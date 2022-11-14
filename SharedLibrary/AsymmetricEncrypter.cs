using System.Security.Cryptography;
using System.Text;

namespace SharedLibrary;

public class AsymmetricEncrypter
{
    private RSACryptoServiceProvider rsa;
    private string publicKey;
    private string privateKey;
    private string otherPublicKey;

    public AsymmetricEncrypter()
    {
        rsa = new RSACryptoServiceProvider(2048);
        publicKey = rsa.ToXmlString(false);
        privateKey = rsa.ToXmlString(true);
        
    }

    public string PublicKey
    {
        get { return publicKey; }
    }

    public string PrivateKey
    {
        get { return privateKey; }
    }
    
    public string OtherPublicKey
    {
        get { return otherPublicKey; }
        set
        {
            otherPublicKey = value;
        }
    }

    public string Encrypt(string data)
    {
        rsa.FromXmlString(otherPublicKey);
        byte[] dataBytes = Encoding.UTF8.GetBytes(data);
        byte[] encryptedBytes = rsa.Encrypt(dataBytes, true);
        return Convert.ToBase64String(encryptedBytes);
    }

    public string Decrypt(string data)
    {
        rsa.FromXmlString(privateKey);
        byte[] dataBytes = Convert.FromBase64String(data);
        byte[] decryptedBytes = rsa.Decrypt(dataBytes, true);
        return Encoding.UTF8.GetString(decryptedBytes);
    }
}