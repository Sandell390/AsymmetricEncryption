using System.Diagnostics;
using System.Security.Cryptography;

namespace SharedLibrary;

public class SymmetricEncrypter
{
    public EncryptData EncryptData { get; set; }
    public List<TimeSpan> TimeSpans { get; }

    private SymmetricAlgorithm _symmetricAlgorithm;
    public KeySizes LegacyKeySize { get; private set; }
    public KeySizes LegacyBlockSize { get; private set; }

    public SymmetricEncrypter(EncryptData encryptData)
    {
        EncryptData = encryptData;
        switch (encryptData.AlgorithmType)
        {
            case EncryptData.Algorithm.Aes:
                _symmetricAlgorithm = Aes.Create();
                break;
            case EncryptData.Algorithm.TripleDes:
                _symmetricAlgorithm = TripleDES.Create();
                break;
            case EncryptData.Algorithm.Des:
                _symmetricAlgorithm = DES.Create();
                break;
            default:
                _symmetricAlgorithm = Aes.Create();
                break;
        }

        TimeSpans = new List<TimeSpan>();
        LegacyBlockSize = _symmetricAlgorithm.LegalBlockSizes[0];
        LegacyKeySize = _symmetricAlgorithm.LegalKeySizes[0];
    }

    public string Decrypt(byte[] cipherText)
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();
        string plaintext;


        using (SymmetricAlgorithm algorithm = _symmetricAlgorithm)
        {
            algorithm.Mode = EncryptData.Mode;
            algorithm.BlockSize = EncryptData.BlockSize;
            algorithm.KeySize = EncryptData.KeySize;
            algorithm.Key = EncryptData.Key;
            algorithm.IV = EncryptData.IV;
            algorithm.Padding = PaddingMode.PKCS7;

            ICryptoTransform decryptor = algorithm.CreateDecryptor(_symmetricAlgorithm.Key, _symmetricAlgorithm.IV);

            using (MemoryStream msDecrypt = new MemoryStream(cipherText))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {
                        plaintext = srDecrypt.ReadToEnd();
                    }
                }
            }
        }
        
        sw.Stop();
        TimeSpans.Add(sw.Elapsed);
        return plaintext;
    }

    public byte[] Encrypt(string plainText)
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();
        byte[] encrypted;


        using (SymmetricAlgorithm algorithm = _symmetricAlgorithm)
        {
            algorithm.Mode = EncryptData.Mode;
            algorithm.BlockSize = EncryptData.BlockSize;
            algorithm.KeySize = EncryptData.KeySize;
            algorithm.Key = EncryptData.Key;
            algorithm.IV = EncryptData.IV;
            algorithm.Padding = PaddingMode.PKCS7;

            ICryptoTransform encryptor = algorithm.CreateEncryptor(_symmetricAlgorithm.Key, _symmetricAlgorithm.IV);

            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(plainText);
                    }
                    encrypted = msEncrypt.ToArray();
                }
            }
        }

        sw.Stop();
        TimeSpans.Add(sw.Elapsed);
        return encrypted;
    }

    public void GenerateKey()
    {
        _symmetricAlgorithm.GenerateIV();
        _symmetricAlgorithm.GenerateKey();
        EncryptData.IV = _symmetricAlgorithm.IV;
        EncryptData.Key = _symmetricAlgorithm.Key;
    }
}