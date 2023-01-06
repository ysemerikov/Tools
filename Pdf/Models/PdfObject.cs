namespace Pdf.Models;

public class PdfObject : PdfElementBase
{
    public readonly int OrderNumber;
    public readonly int GenerationNumber;
    public readonly byte[]? StreamBytes;

    public PdfObject(byte[] raw, in int orderNumber, in int generationNumber, byte[]? streamBytes)
        : base(raw)
    {
        OrderNumber = orderNumber;
        GenerationNumber = generationNumber;
        StreamBytes = streamBytes;
    }
}