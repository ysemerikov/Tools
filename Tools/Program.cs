using Pdf;

namespace Tools;

internal class Program
{
    private static void Main(string[] args)
    {
        var doc = new PdfReader().Read(@"C:\Users\ysemerikov\YandexDisk\Документы\ИП\2018-deklar.pdf");
        Console.WriteLine(doc.Objects.Count);
    }
}