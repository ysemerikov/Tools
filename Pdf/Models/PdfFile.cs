namespace Pdf.Models;

public class PdfFile
{
    public PdfHeader Header { get; }
    public List<PdfObject> Objects { get; }
    public PdfXref Xref { get; }
    public PdfTrailer Trailer { get; }

    public PdfFile(PdfHeader header, List<PdfObject> objects, PdfXref xref, PdfTrailer trailer)
    {
        Header = header;
        Objects = objects;
        Xref = xref;
        Trailer = trailer;
    }
}