using System.Security.Cryptography;

namespace SharedLibrary;

public class EncryptData
{
    public int BlockSize { get; private set; }
    public int KeySize { get; private set; }
    
    public CipherMode Mode { get; private set; }

    public Algorithm AlgorithmType { get; private set; }
    
    public byte[] IV { get; set; }
    
    public byte[] Key { get; set; }
    
    public enum Algorithm
    {
        Aes = 1,
        TripleDes = 2,
        Des = 3,
    }
    
    public EncryptData(Algorithm algorithmType, CipherMode mode, int keySize, int blockSize)
    {
        AlgorithmType = algorithmType;
        Mode = mode;
        KeySize = keySize;
        BlockSize = blockSize;
    }

}