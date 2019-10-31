using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Tools.Core.Actions.NetworkActions
{
    public class TcpRequestSender : IAction
    {
        public async Task Do()
        {
            using var client = new TcpClient();
            client.ReceiveTimeout = 1000;
            client.SendTimeout = 1000;
            await client.ConnectAsync("google.com", 80);
            using var stream = client.GetStream();

            var toSend = Encoding.ASCII.GetBytes("GET / HTTP/1.1\r\nConnection: close\r\n\r\n");
            await stream.WriteAsync(toSend, 0, toSend.Length);
            await stream.FlushAsync();

//            await Task.Delay(1000);
            var filename = $"{DateTime.Now:HH-mm-ss.ff}";
            await stream.CopyToAsync(File.Open($@"C:\servicetitan\00_{filename}.txt", FileMode.Create));
            Console.WriteLine(stream.DataAvailable);
        }
    }
}