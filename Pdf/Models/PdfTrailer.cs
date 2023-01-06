namespace Pdf.Models;

public class PdfTrailer : PdfElementBase
{
    public readonly int StartXref;

    public PdfTrailer(byte[] raw, int startxref) : base(raw)
    {
        StartXref = startxref;
    }
}