using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Pdf.Models;

namespace Pdf;

internal class PdfReaderInternal
{
    private readonly ICustomStreamReader reader;

    public PdfReaderInternal(byte[] bytes)
    {
        reader = new CustomStreamReader(bytes);
    }

    public PdfHeader ReadPdfHeader()
    {
        if (reader.Position != 0)
            throw new Exception($"calling {nameof(ReadPdfHeader)} isn't expected.");

        var length = reader.Forget(r =>
        {
            if (!r.ReadLine().StartsWith("%PDF") || r.ReadLine().Length == 0)
                throw new Exception("isn't PDF format.");
        });

        return new PdfHeader(reader.ReadBytes(length));
    }

    public bool IsNextObject()
    {
        using (reader.Forget())
            return TryReadObjectHeader(out _, out _);
    }

    public PdfObject ReadObject()
    {
        var streamBytes = default(byte[]);
        var orderNumber = -1;
        var gen = -1;

        var length = reader.Forget(_ =>
        {
            if (!TryReadObjectHeader(out orderNumber, out gen))
                throw new Exception("can't read object header.");

            var data = ReadObjectData();

            if (reader.TryReadLine("stream"))
            {
                var dataLength = data.GetLength();
                streamBytes = reader.ReadBytes(dataLength);
                reader.TryReadLine();
                if (!reader.TryReadLine("endstream"))
                    throw new Exception($"expected endstream of {orderNumber} {gen} obj. Length = {dataLength}.");
            }

            if (!reader.TryReadLine("endobj"))
                throw new Exception($"expected endobj of {orderNumber} {gen} obj.");
        });

        return new PdfObject(reader.ReadBytes(length), orderNumber, gen, streamBytes);
    }

    private bool TryReadObjectHeader(out int orderNumber, out int generationNumber)
    {
        var forgotter = reader.Forget();

        generationNumber = 0;
        if (reader.TryReadPositiveInt(out orderNumber)
            && reader.TryRead(" ")
            && reader.TryReadPositiveInt(out generationNumber)
            && reader.TryReadLine(" obj"))
            return true;

        forgotter.Dispose();
        return false;
    }

    private ObjectData ReadObjectData()
    {
        var length = reader.Forget(r =>
        {
            if (reader.TryRead(" ") && reader.TryReadPositiveInt(out _) && reader.TryReadLine())
                return;

            if (!TryReadObjectDataStart(out var end))
                throw new Exception($"can't start to read obj data, position = {reader.Position}.");

            var stack = new Stack<string>();
            stack.Push(end);

            while (stack.Count > 0)
                if (reader.TryRead(stack.Peek()))
                    stack.Pop();
                else if (TryReadObjectDataStart(out end))
                    stack.Push(end);
                else
                    reader.ReadBytes(1);

            reader.TryReadLine();
        });

        return new ObjectData(reader.ReadBytes(length));
    }

    private bool TryReadObjectDataStart([NotNullWhen(true)] out string? end)
    {
        if (reader.TryRead("<<"))
        {
            end = ">>";
            return true;
        }

        if (reader.TryRead("["))
        {
            end = "]";
            return true;
        }

        end = null;
        return false;
    }

    public PdfXref ReadXref()
    {
        var lines = default(List<PdfXrefLine>?);
        var length = reader.Forget(_ =>
        {
            if (!reader.TryReadLine("xref"))
                throw new Exception($"xref is expected, position = {reader.Position}");
            if (!reader.TryRead("0 "))
                throw new Exception("check xref");

            if (!reader.TryReadPositiveInt(out var count) || !reader.TryReadLine())
                throw new Exception("expected number of objects.");

            lines = Enumerable
                .Range(1, count)
                .Select(i =>
                {
                    if (
                        !reader.TryReadPositiveInt(out var shift)
                        || !reader.TryRead(" ")
                        || !reader.TryReadPositiveInt(out var gen)
                        || !reader.TryRead(" ")
                    )
                        throw new Exception("xref 1");

                    var f = reader.TryReadLine("n ") || reader.TryReadLine("n")
                        ? 'n'
                        : reader.TryReadLine("f ") || reader.TryReadLine("f")
                            ? 'f'
                            : throw new Exception("xref f|n");

                    return new PdfXrefLine(i - 1, shift, gen, f);
                })
                .ToList();
        });

        return new PdfXref(reader.ReadBytes(length), lines);
    }

    public PdfTrailer ReadTrailer()
    {
        var startxref = -1;

        var length = reader.Forget(_ =>
        {
            if (!reader.TryReadLine("trailer"))
                throw new Exception($"trailer is expected, position = {reader.Position}");
            ReadObjectData();
            if (!reader.TryReadLine("startxref"))
                throw new Exception($"startxref is expected, position = {reader.Position}");

            if (!reader.TryReadPositiveInt(out startxref) || !reader.TryReadLine())
                throw new Exception($"length is expected, position = {reader.Position}");
            if (!reader.TryReadLine("%%EOF"))
                throw new Exception($"EOF is expected, position = {reader.Position}");

            if (!reader.IsFinished())
                throw new Exception("pdf file has more data than expected.");
        });

        return new PdfTrailer(reader.ReadBytes(length), startxref);
    }

    private class ObjectData
    {
        private readonly string str;

        public ObjectData(byte[] raw)
        {
            str = new string(raw.Select(x => (char) x).ToArray());
        }

        public int GetLength()
        {
            if (!TryGet("Length", out var s) || !int.TryParse(s, out var length))
                throw new Exception(str);
            return length;
        }

        private bool TryGet(string key, [NotNullWhen(true)] out string? value)
        {
            var toFind = $"/{key} ";
            var index = str.IndexOf(toFind, StringComparison.InvariantCulture);
            if (index < 0)
            {
                value = null;
                return false;
            }

            if (index != str.LastIndexOf(toFind, StringComparison.InvariantCulture))
                throw new NotSupportedException($"We do not support dict inside dict.\r\n{str}");

            var since = index + toFind.Length;
            for (var to = since; to < str.Length; to++)
            {
                var c = str[to];
                if (c == ' ' || c == '\n' || c == '/' || c == '>')
                {
                    value = str.Substring(since, to - since);
                    return true;
                }
            }

            throw new Exception("not possible");
        }
    }
}
