using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using SharedLibrary;

namespace SharedLibrary;

public class WebSocketHandler
{
    private WebSocket webSocket;
    
    AsymmetricEncrypter asymmetricEncrypter;
    SymmetricEncrypter symmetricEncrypter;
    
    bool hasSentSymmetricKey = false;
    bool hasReceivedSymmetricKey = false;
    
    public WebSocketHandler(WebSocket _webSocket)
    {
        asymmetricEncrypter = new AsymmetricEncrypter();
        webSocket = _webSocket;
        Receive();
        Send(JsonConvert.SerializeObject(new Package("only encrypt", "")));
    }
    
    public async void Receive()
    {
        while (webSocket.State == WebSocketState.Open)
        {
            var buffer = new byte[1024 * 4];
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Text)
            {
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                Package? package = null;
                try
                {
                    package = JsonConvert.DeserializeObject<Package>(message);
                }
                catch (Exception e)
                {
                    
                }
                
                if (package != null)
                {
                    
                    switch (package.Action)
                    {
                        case "only encrypt":
                            Console.WriteLine("Sent public key: " + asymmetricEncrypter.PublicKey);
                            Send(JsonConvert.SerializeObject(new Package("GetPublicKey", asymmetricEncrypter.PublicKey)));
                            break;
                        case "GetPublicKey":
                            asymmetricEncrypter.OtherPublicKey = package.Message;
                            Console.WriteLine("Received public key: " + package.Message);
                            Send(asymmetricEncrypter.Encrypt(JsonConvert.SerializeObject(new Package("RequestSymmetricEncrypt", ""))));
                            Console.WriteLine("Requested encrypted symmetric key");
                            break;
                        default:
                            break;
                    }
                }
                else if (!hasReceivedSymmetricKey)
                {
                    try
                    {
                        string decryptedMessage = asymmetricEncrypter.Decrypt(message);
                        package = JsonConvert.DeserializeObject<Package>(decryptedMessage);
                        if (package != null)
                        {
                            switch (package.Action)
                            {
                                case "GetSymmetricEncrypt":
                                    Console.WriteLine("Received symmetric key");
                                    EncryptData encryptData = JsonConvert.DeserializeObject<EncryptData>(package.Message);
                                    symmetricEncrypter = new SymmetricEncrypter(encryptData);
                                    hasReceivedSymmetricKey = true;
                                    Console.WriteLine("Symmetric key: " + Convert.ToBase64String(symmetricEncrypter.EncryptData.Key));
                                    Console.WriteLine("Symmetric IV: " + Convert.ToBase64String(symmetricEncrypter.EncryptData.IV));
                                    Send(Convert.ToBase64String(symmetricEncrypter.Encrypt(JsonConvert.SerializeObject(new Package("message", "Kage")))));
                                    break;
                                default:
                                    Send("error");
                                    break;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                   
                }else
                {
                    string decryptedMessage = symmetricEncrypter.Decrypt(Convert.FromBase64String(message));
                    package = JsonConvert.DeserializeObject<Package>(decryptedMessage);
                    if (package != null)
                    {
                        switch (package.Action)
                        {
                            case "message":
                                Console.WriteLine("Received: " + package.Message);
                                byte[] encryptedMessage = symmetricEncrypter.Encrypt(JsonConvert.SerializeObject(new Package("message", "kage")));
                                Send(Convert.ToBase64String(encryptedMessage));
                                break;
                            default:
                                Send("error");
                                break;
                        }
                    }
                }

            }
            else if (result.MessageType == WebSocketMessageType.Close)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
            }
        }
    }
    
    public async void Send(string message)
    {
        var buffer = Encoding.UTF8.GetBytes(message);
        await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
    }
}