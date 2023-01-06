using System;
using System.Collections.Generic;
using System.IO;
using Pdf.Models;

namespace Pdf;

public class PdfReader
{
    public PdfFile Read(string path)
    {
        var bytes = File.ReadAllBytes(path);
        var reader = new PdfReaderInternal(bytes);
        var header = reader.ReadPdfHeader();
        var objects = new List<PdfObject>();
        while (reader.IsNextObject())
            objects.Add(reader.ReadObject());
        var xref = reader.ReadXref();
        var trailer = reader.ReadTrailer();

        var pdf = new PdfFile(header, objects, xref, trailer);
        CheckPdf(pdf);
        return pdf;
    }

    private static void CheckPdf(PdfFile pdf)
    {
        var realShifts = new Dictionary<int, int> {{0, 0}};
        var current = pdf.Header.Binary.Length;
        foreach (var o in pdf.Objects)
        {
            realShifts[o.OrderNumber] = current;
            current += o.Binary.Length;
        }

        if (pdf.Trailer.StartXref != current)
            throw new Exception("Check StartXref is wrong.");
        foreach (var line in pdf.Xref.Lines)
            if (line.Shift != realShifts[line.OrderNumber])
                throw new Exception($"Check ObjectShift {line.OrderNumber} is wrong.");
    }
}
