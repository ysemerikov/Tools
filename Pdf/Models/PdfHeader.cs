namespace Pdf.Models;

public class PdfHeader : PdfElementBase
{
    private readonly byte[] bytes;

    public PdfHeader(byte[] bytes)
        : base(bytes)
    {
        this.bytes = bytes;
    }
}