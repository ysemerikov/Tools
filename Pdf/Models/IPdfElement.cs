namespace Pdf.Models;

public interface IPdfElement
{
    public byte[] Binary { get; }
}

public abstract class PdfElementBase : IPdfElement
{
    protected PdfElementBase(byte[] raw)
    {
        Binary = raw;
    }

    public byte[] Binary { get; }
}