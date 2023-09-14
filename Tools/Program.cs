using System.Threading.Tasks;
using Tools.Actions.BinaryFilesActions;
using Tools.Actions.TextFileActions;

namespace Tools;

internal class Program
{
    private static async Task Main(string[] args)
    {
        // var doc = new PdfReader().Read(@"C:\Users\ysemerikov\YandexDisk\Документы\ИП\2018-deklar.pdf");
        // Console.WriteLine(doc.Objects.Count);

        // await new ThmInvoices(@"D:/Downloads/invoices 2023-06").Do();
        await new TextSearch("rabbitmq-secret", @"D:\thm\k8s-production").Do();
    }
}
