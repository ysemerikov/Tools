namespace Pdf.Models;

public class PdfXref : PdfElementBase
{
    public readonly List<PdfXrefLine> Lines;

    public PdfXref(byte[] raw, List<PdfXrefLine>? lines)
        : base(raw)
    {
        Lines = lines ?? new List<PdfXrefLine>();
    }
}

public class PdfXrefLine
{
    public int OrderNumber;
    public readonly int Shift;
    public readonly int Gen;
    public readonly char F;

    public PdfXrefLine(in int orderNumber, in int shift, in int gen, in char f)
    {
        OrderNumber = orderNumber;
        Shift = shift;
        Gen = gen;
        F = f;
    }
}
