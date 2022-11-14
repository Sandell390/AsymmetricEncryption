// See https://aka.ms/new-console-template for more information

using System.Net.WebSockets;
using System.Text;
using SharedLibrary;

Console.Title = "Client";
Thread.Sleep(3000);
using (var ws = new ClientWebSocket())
{
    await ws.ConnectAsync(new Uri("ws://localhost:6666/ws"), CancellationToken.None);
    Console.WriteLine("Connected");
    
    WebSocketHandler handler = new WebSocketHandler(ws);
    while (ws.State == WebSocketState.Open)
    {
        
    }

}

Console.ReadLine();