

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SebWindowsClient.CryptographyUtils
{
  public static class AESThenHMAC
  {
    private static readonly RandomNumberGenerator Random = RandomNumberGenerator.Create();
    public static readonly int BlockBitSize = 128;
    public static readonly int KeyBitSize = 256;
    public static readonly int SaltBitSize = 64;
    public static readonly int Iterations = 10000;
    public static readonly int MinPasswordLength = 3;

    public static byte[] NewKey()
    {
      byte[] data = new byte[AESThenHMAC.KeyBitSize / 8];
      AESThenHMAC.Random.GetBytes(data);
      return data;
    }

    public static string SimpleEncrypt(string secretMessage, byte[] cryptKey, byte[] authKey, byte[] nonSecretPayload = null)
    {
      if (string.IsNullOrEmpty(secretMessage))
        throw new ArgumentException("Secret Message Required!", nameof (secretMessage));
      return Convert.ToBase64String(AESThenHMAC.SimpleEncrypt(Encoding.UTF8.GetBytes(secretMessage), cryptKey, authKey, nonSecretPayload));
    }

    public static string SimpleDecrypt(string encryptedMessage, byte[] cryptKey, byte[] authKey, int nonSecretPayloadLength = 0)
    {
      if (string.IsNullOrWhiteSpace(encryptedMessage))
        throw new ArgumentException("Encrypted Message Required!", nameof (encryptedMessage));
      return Encoding.UTF8.GetString(AESThenHMAC.SimpleDecrypt(Convert.FromBase64String(encryptedMessage), cryptKey, authKey, nonSecretPayloadLength));
    }

    public static string SimpleEncryptWithPassword(string secretMessage, string password, byte[] nonSecretPayload = null)
    {
      if (string.IsNullOrEmpty(secretMessage))
        throw new ArgumentException("Secret Message Required!", nameof (secretMessage));
      return Convert.ToBase64String(AESThenHMAC.SimpleEncryptWithPassword(Encoding.UTF8.GetBytes(secretMessage), password, nonSecretPayload));
    }

    public static string SimpleDecryptWithPassword(string encryptedMessage, string password, int nonSecretPayloadLength = 0)
    {
      if (string.IsNullOrWhiteSpace(encryptedMessage))
        throw new ArgumentException("Encrypted Message Required!", nameof (encryptedMessage));
      return Encoding.UTF8.GetString(AESThenHMAC.SimpleDecryptWithPassword(Convert.FromBase64String(encryptedMessage), password, nonSecretPayloadLength));
    }

    public static byte[] SimpleEncrypt(byte[] secretMessage, byte[] cryptKey, byte[] authKey, byte[] nonSecretPayload = null)
    {
      if (cryptKey == null || cryptKey.Length != AESThenHMAC.KeyBitSize / 8)
        throw new ArgumentException(string.Format("Key needs to be {0} bit!", (object) AESThenHMAC.KeyBitSize), nameof (cryptKey));
      if (authKey == null || authKey.Length != AESThenHMAC.KeyBitSize / 8)
        throw new ArgumentException(string.Format("Key needs to be {0} bit!", (object) AESThenHMAC.KeyBitSize), nameof (authKey));
      if (secretMessage == null || secretMessage.Length < 1)
        throw new ArgumentException("Secret Message Required!", nameof (secretMessage));
      nonSecretPayload = nonSecretPayload ?? new byte[0];
      AesManaged aesManaged1 = new AesManaged();
      int keyBitSize = AESThenHMAC.KeyBitSize;
      aesManaged1.KeySize = keyBitSize;
      int blockBitSize = AESThenHMAC.BlockBitSize;
      aesManaged1.BlockSize = blockBitSize;
      int num1 = 1;
      aesManaged1.Mode = (CipherMode) num1;
      int num2 = 2;
      aesManaged1.Padding = (PaddingMode) num2;
      byte[] iv;
      byte[] array;
      using (AesManaged aesManaged2 = aesManaged1)
      {
        aesManaged2.GenerateIV();
        iv = aesManaged2.IV;
        using (ICryptoTransform encryptor = aesManaged2.CreateEncryptor(cryptKey, iv))
        {
          using (MemoryStream memoryStream = new MemoryStream())
          {
            using (CryptoStream cryptoStream = new CryptoStream((Stream) memoryStream, encryptor, CryptoStreamMode.Write))
            {
              using (BinaryWriter binaryWriter = new BinaryWriter((Stream) cryptoStream))
                binaryWriter.Write(secretMessage);
            }
            array = memoryStream.ToArray();
          }
        }
      }
      using (HMACSHA256 hmacshA256 = new HMACSHA256(authKey))
      {
        using (MemoryStream memoryStream = new MemoryStream())
        {
          using (BinaryWriter binaryWriter = new BinaryWriter((Stream) memoryStream))
          {
            binaryWriter.Write(nonSecretPayload);
            binaryWriter.Write(iv);
            binaryWriter.Write(array);
            binaryWriter.Flush();
            byte[] hash = hmacshA256.ComputeHash(memoryStream.ToArray());
            binaryWriter.Write(hash);
          }
          return memoryStream.ToArray();
        }
      }
    }

    public static byte[] SimpleDecrypt(byte[] encryptedMessage, byte[] cryptKey, byte[] authKey, int nonSecretPayloadLength = 0)
    {
      if (cryptKey == null || cryptKey.Length != AESThenHMAC.KeyBitSize / 8)
        throw new ArgumentException(string.Format("CryptKey needs to be {0} bit!", (object) AESThenHMAC.KeyBitSize), nameof (cryptKey));
      if (authKey == null || authKey.Length != AESThenHMAC.KeyBitSize / 8)
        throw new ArgumentException(string.Format("AuthKey needs to be {0} bit!", (object) AESThenHMAC.KeyBitSize), nameof (authKey));
      if (encryptedMessage == null || encryptedMessage.Length == 0)
        throw new ArgumentException("Encrypted Message Required!", nameof (encryptedMessage));
      using (HMACSHA256 hmacshA256 = new HMACSHA256(authKey))
      {
        byte[] numArray = new byte[hmacshA256.HashSize / 8];
        byte[] hash = hmacshA256.ComputeHash(encryptedMessage, 0, encryptedMessage.Length - numArray.Length);
        int length = AESThenHMAC.BlockBitSize / 8;
        if (encryptedMessage.Length < numArray.Length + nonSecretPayloadLength + length)
          return (byte[]) null;
        Array.Copy((Array) encryptedMessage, encryptedMessage.Length - numArray.Length, (Array) numArray, 0, numArray.Length);
        int num1 = 0;
        for (int index = 0; index < numArray.Length; ++index)
          num1 |= (int) numArray[index] ^ (int) hash[index];
        if (num1 != 0)
          return (byte[]) null;
        AesManaged aesManaged1 = new AesManaged();
        int keyBitSize = AESThenHMAC.KeyBitSize;
        aesManaged1.KeySize = keyBitSize;
        int blockBitSize = AESThenHMAC.BlockBitSize;
        aesManaged1.BlockSize = blockBitSize;
        int num2 = 1;
        aesManaged1.Mode = (CipherMode) num2;
        int num3 = 2;
        aesManaged1.Padding = (PaddingMode) num3;
        using (AesManaged aesManaged2 = aesManaged1)
        {
          byte[] rgbIV = new byte[length];
          Array.Copy((Array) encryptedMessage, nonSecretPayloadLength, (Array) rgbIV, 0, rgbIV.Length);
          using (ICryptoTransform decryptor = aesManaged2.CreateDecryptor(cryptKey, rgbIV))
          {
            using (MemoryStream memoryStream = new MemoryStream())
            {
              using (CryptoStream cryptoStream = new CryptoStream((Stream) memoryStream, decryptor, CryptoStreamMode.Write))
              {
                using (BinaryWriter binaryWriter = new BinaryWriter((Stream) cryptoStream))
                  binaryWriter.Write(encryptedMessage, nonSecretPayloadLength + rgbIV.Length, encryptedMessage.Length - nonSecretPayloadLength - rgbIV.Length - numArray.Length);
              }
              return memoryStream.ToArray();
            }
          }
        }
      }
    }

    public static byte[] SimpleEncryptWithPassword(byte[] secretMessage, string password, byte[] nonSecretPayload = null)
    {
      nonSecretPayload = nonSecretPayload ?? new byte[0];
      if (secretMessage == null || secretMessage.Length == 0)
        throw new ArgumentException("Secret Message Required!", nameof (secretMessage));
      byte[] nonSecretPayload1 = new byte[AESThenHMAC.SaltBitSize / 8 * 2 + nonSecretPayload.Length];
      Array.Copy((Array) nonSecretPayload, (Array) nonSecretPayload1, nonSecretPayload.Length);
      int length = nonSecretPayload.Length;
      byte[] bytes1;
      using (Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, AESThenHMAC.SaltBitSize / 8, AESThenHMAC.Iterations))
      {
        byte[] salt = rfc2898DeriveBytes.Salt;
        bytes1 = rfc2898DeriveBytes.GetBytes(AESThenHMAC.KeyBitSize / 8);
        Array.Copy((Array) salt, 0, (Array) nonSecretPayload1, length, salt.Length);
        length += salt.Length;
      }
      byte[] bytes2;
      using (Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, AESThenHMAC.SaltBitSize / 8, AESThenHMAC.Iterations))
      {
        byte[] salt = rfc2898DeriveBytes.Salt;
        bytes2 = rfc2898DeriveBytes.GetBytes(AESThenHMAC.KeyBitSize / 8);
        Array.Copy((Array) salt, 0, (Array) nonSecretPayload1, length, salt.Length);
      }
      return AESThenHMAC.SimpleEncrypt(secretMessage, bytes1, bytes2, nonSecretPayload1);
    }

    public static byte[] SimpleDecryptWithPassword(byte[] encryptedMessage, string password, int nonSecretPayloadLength = 0)
    {
      if (encryptedMessage == null || encryptedMessage.Length == 0)
        throw new ArgumentException("Encrypted Message Required!", nameof (encryptedMessage));
      byte[] salt1 = new byte[AESThenHMAC.SaltBitSize / 8];
      byte[] salt2 = new byte[AESThenHMAC.SaltBitSize / 8];
      Array.Copy((Array) encryptedMessage, nonSecretPayloadLength, (Array) salt1, 0, salt1.Length);
      Array.Copy((Array) encryptedMessage, nonSecretPayloadLength + salt1.Length, (Array) salt2, 0, salt2.Length);
      byte[] bytes1;
      using (Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, salt1, AESThenHMAC.Iterations))
        bytes1 = rfc2898DeriveBytes.GetBytes(AESThenHMAC.KeyBitSize / 8);
      byte[] bytes2;
      using (Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, salt2, AESThenHMAC.Iterations))
        bytes2 = rfc2898DeriveBytes.GetBytes(AESThenHMAC.KeyBitSize / 8);
      return AESThenHMAC.SimpleDecrypt(encryptedMessage, bytes1, bytes2, salt1.Length + salt2.Length + nonSecretPayloadLength);
    }
  }
}
