
using SebWindowsClient.ConfigurationUtils;
using SebWindowsClient.DiagnosticsUtils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Windows.Forms;

namespace SebWindowsClient.CryptographyUtils
{
  public class SEBProtectionController
  {
    private static byte[] RNCRYPTOR_HEADER = new byte[2]
    {
      (byte) 2,
      (byte) 1
    };
    private const int PREFIX_LENGTH = 4;
    private const string PUBLIC_KEY_HASH_MODE = "pkhs";
    private const string PASSWORD_MODE = "pswd";
    private const string PLAIN_DATA_MODE = "plnd";
    private const string PASSWORD_CONFIGURING_CLIENT_MODE = "pwcc";
    private const int PUBLIC_KEY_HASH_LENGTH = 20;

    public static ArrayList GetCertificatesAndNames(ref ArrayList certificateNames)
    {
      ArrayList arrayList = new ArrayList();
      X509Store store1 = new X509Store(StoreLocation.CurrentUser);
      ArrayList andNamesFromStore1 = SEBProtectionController.GetCertificatesAndNamesFromStore(ref certificateNames, store1);
      ArrayList certificateNames1 = new ArrayList();
      X509Store store2 = new X509Store(StoreLocation.LocalMachine);
      ArrayList andNamesFromStore2 = SEBProtectionController.GetCertificatesAndNamesFromStore(ref certificateNames1, store2);
      andNamesFromStore1.AddRange((ICollection) andNamesFromStore2);
      certificateNames.AddRange((ICollection) certificateNames1);
      return andNamesFromStore1;
    }

    public static ArrayList GetCertificatesAndNamesFromStore(ref ArrayList certificateNames, X509Store store)
    {
      ArrayList arrayList = new ArrayList();
      store.Open(OpenFlags.ReadOnly);
      foreach (X509Certificate2 certificate in store.Certificates)
      {
        if (certificate.HasPrivateKey)
        {
          arrayList.Add((object) certificate);
          if (!string.IsNullOrWhiteSpace(certificate.FriendlyName))
            certificateNames.Add((object) certificate.FriendlyName);
          else if (!string.IsNullOrWhiteSpace(certificate.SerialNumber))
            certificateNames.Add((object) certificate.SerialNumber);
        }
      }
      store.Close();
      return arrayList;
    }

    public static ArrayList GetSSLCertificatesAndNames(ref ArrayList certificateNames)
    {
      ArrayList arrayList = new ArrayList();
      X509Store x509Store1 = new X509Store(StoreName.CertificateAuthority);
      x509Store1.Open(OpenFlags.ReadOnly);
      X509Certificate2Collection certificate2Collection = x509Store1.Certificates.Find(X509FindType.FindByKeyUsage, (object) X509KeyUsageFlags.DigitalSignature, false);
      X509Store x509Store2 = new X509Store(StoreName.AddressBook);
      x509Store2.Open(OpenFlags.ReadOnly);
      X509Certificate2Collection certificates = x509Store2.Certificates.Find(X509FindType.FindByKeyUsage, (object) X509KeyUsageFlags.DigitalSignature, false);
      certificate2Collection.AddRange(certificates);
      foreach (X509Certificate2 x509Certificate2 in certificate2Collection)
      {
        arrayList.Add((object) x509Certificate2);
        certificateNames.Add((object) SEBProtectionController.Parse(x509Certificate2.Subject, "CN").FirstOrDefault<string>());
      }
      x509Store2.Close();
      return arrayList;
    }

    public static List<string> Parse(string data, string delimiter)
    {
      if (data == null)
        return (List<string>) null;
      if (!delimiter.EndsWith("="))
        delimiter += "=";
      if (!data.Contains(delimiter))
        return (List<string>) null;
      List<string> stringList1 = new List<string>();
      int startIndex = data.IndexOf(delimiter) + delimiter.Length;
      int length = data.IndexOf(',', startIndex) - startIndex;
      if (length == 0)
        return (List<string>) null;
      if (length > 0)
      {
        stringList1.Add(data.Substring(startIndex, length));
        List<string> stringList2 = SEBProtectionController.Parse(data.Substring(startIndex + length), delimiter);
        if (stringList2 != null)
          stringList1.AddRange((IEnumerable<string>) stringList2);
      }
      else
        stringList1.Add(data.Substring(startIndex));
      return stringList1;
    }

    public static X509Certificate2 GetCertificateFromStore(byte[] publicKeyHash)
    {
      X509Store store1 = new X509Store(StoreLocation.CurrentUser);
      X509Certificate2 certificateFromPassedStore = SEBProtectionController.GetCertificateFromPassedStore(publicKeyHash, store1);
      if (certificateFromPassedStore == null)
      {
        X509Store store2 = new X509Store(StoreLocation.LocalMachine);
        certificateFromPassedStore = SEBProtectionController.GetCertificateFromPassedStore(publicKeyHash, store2);
      }
      return certificateFromPassedStore;
    }

    public static X509Certificate2 GetCertificateFromPassedStore(byte[] publicKeyHash, X509Store store)
    {
      X509Certificate2 x509Certificate2 = (X509Certificate2) null;
      store.Open(OpenFlags.ReadOnly);
      foreach (X509Certificate2 certificate in store.Certificates)
      {
        if (((IEnumerable<byte>) new SHA1CryptoServiceProvider().ComputeHash(certificate.PublicKey.EncodedKeyValue.RawData)).SequenceEqual<byte>((IEnumerable<byte>) publicKeyHash))
        {
          x509Certificate2 = certificate;
          break;
        }
      }
      store.Close();
      return x509Certificate2;
    }

    public static void StoreCertificateIntoStore(byte[] certificateData)
    {
      X509Certificate2 certificate = new X509Certificate2();
      X509Store x509Store;
      try
      {
        x509Store = new X509Store(StoreLocation.CurrentUser);
        x509Store.Open(OpenFlags.ReadWrite);
      }
      catch (Exception ex)
      {
        Logger.AddError("The X509 store in Windows Certificate Store could not be opened: ", (object) null, ex, ex.Message);
        return;
      }
      try
      {
        certificate.Import(certificateData, "Di\xD834\xDE2Dl\xD834\xDE16Ch\xD834\xDE12ah\xD834\xDE47t\xD834\xDE01a\xD834\xDE48Hai1972", X509KeyStorageFlags.UserKeySet | X509KeyStorageFlags.PersistKeySet);
      }
      catch (Exception ex)
      {
        Logger.AddError("The identity data could not be imported into the X509 certificate store.", (object) null, ex, ex.Message);
        x509Store.Close();
        return;
      }
      try
      {
        x509Store.Add(certificate);
      }
      catch (Exception ex)
      {
        Logger.AddError("The identity could not be added to the Windows Certificate Store", (object) null, ex, ex.Message);
        x509Store.Close();
        return;
      }
      Logger.AddInformation("The identity was successfully added to the Windows Certificate Store", (object) null, (Exception) null, (string) null);
      x509Store.Close();
    }

    public static byte[] GetPublicKeyHashFromCertificate(X509Certificate2 certificateRef)
    {
      return new SHA1CryptoServiceProvider().ComputeHash(certificateRef.PublicKey.EncodedKeyValue.RawData);
    }

    public static byte[] EncryptDataWithCertificate(byte[] plainInputData, X509Certificate2 sebCertificate)
    {
      try
      {
        RSACryptoServiceProvider cryptoServiceProvider = new RSACryptoServiceProvider();
        RSACryptoServiceProvider key = sebCertificate.PublicKey.Key as RSACryptoServiceProvider;
        int count1 = key.KeySize / 8 - 32;
        byte[] numArray = new byte[count1];
        byte[] rgb1 = new byte[count1];
        MemoryStream memoryStream = new MemoryStream();
        int num = plainInputData.Length / count1;
        for (int index = 0; index < num; ++index)
        {
          Buffer.BlockCopy((Array) plainInputData, index * count1, (Array) rgb1, 0, count1);
          byte[] buffer = key.Encrypt(rgb1, false);
          memoryStream.Write(buffer, 0, buffer.Length);
        }
        int count2 = plainInputData.Length - num * count1;
        if (count2 > 0)
        {
          byte[] rgb2 = new byte[count2];
          Buffer.BlockCopy((Array) plainInputData, num * count1, (Array) rgb2, 0, count2);
          byte[] buffer = key.Encrypt(rgb2, false);
          memoryStream.Write(buffer, 0, buffer.Length);
        }
        return memoryStream.ToArray();
      }
      catch (CryptographicException ex)
      {
        return (byte[]) null;
      }
      catch (Exception ex)
      {
        return (byte[]) null;
      }
    }

    public static byte[] DecryptDataWithCertificate(byte[] encryptedData, X509Certificate2 sebCertificate)
    {
      try
      {
        RSACryptoServiceProvider privateKey = sebCertificate.PrivateKey as RSACryptoServiceProvider;
        int count1 = privateKey.KeySize / 8;
        byte[] rgb1 = new byte[count1];
        byte[] numArray = new byte[count1];
        MemoryStream memoryStream = new MemoryStream();
        int num = encryptedData.Length / count1;
        for (int index = 0; index < num; ++index)
        {
          Buffer.BlockCopy((Array) encryptedData, index * count1, (Array) rgb1, 0, count1);
          byte[] buffer = privateKey.Decrypt(rgb1, false);
          memoryStream.Write(buffer, 0, buffer.Length);
        }
        int count2 = encryptedData.Length - num * count1;
        if (count2 > 0)
        {
          byte[] rgb2 = new byte[count2];
          Buffer.BlockCopy((Array) encryptedData, num * count1, (Array) rgb2, 0, count2);
          byte[] buffer = privateKey.Decrypt(rgb2, false);
          memoryStream.Write(buffer, 0, buffer.Length);
        }
        return memoryStream.ToArray();
      }
      catch (CryptographicException ex)
      {
        Logger.AddError("Decrypting SEB config data encrypted with an identity failed with cryptographic exception:", (object) null, (Exception) ex, ex.Message);
        int num = (int) SEBMessageBox.Show(SEBUIStrings.errorDecryptingSettings, SEBUIStrings.certificateDecryptingError + ex.Message, MessageBoxIcon.Hand, MessageBoxButtons.OK, false);
        return (byte[]) null;
      }
      catch (Exception ex)
      {
        Logger.AddError("Decrypting SEB config data encrypted with an identity failed with exception:", (object) null, ex, ex.Message);
        int num = (int) SEBMessageBox.Show(SEBUIStrings.errorDecryptingSettings, SEBUIStrings.certificateDecryptingError + ex.Message, MessageBoxIcon.Hand, MessageBoxButtons.OK, false);
        return (byte[]) null;
      }
    }

    public static byte[] EncryptDataWithPassword(byte[] plainData, string password)
    {
      try
      {
        return AESThenHMAC.SimpleEncryptWithPassword(plainData, password, SEBProtectionController.RNCRYPTOR_HEADER);
      }
      catch (CryptographicException ex)
      {
        return (byte[]) null;
      }
      catch (Exception ex)
      {
        return (byte[]) null;
      }
    }

    public static byte[] DecryptDataWithPassword(byte[] encryptedBytesWithSalt, string passphrase)
    {
      try
      {
        return AESThenHMAC.SimpleDecryptWithPassword(encryptedBytesWithSalt, passphrase, 2);
      }
      catch (CryptographicException ex)
      {
        return (byte[]) null;
      }
      catch (Exception ex)
      {
        return (byte[]) null;
      }
    }

    public static string ComputePasswordHash(string input)
    {
      return BitConverter.ToString(new SHA256Managed().ComputeHash(Encoding.UTF8.GetBytes(input))).Replace("-", "");
    }

    public static string ComputeBrowserExamKey()
    {
      return "KUEzrcLjW4NZEQkZkzie2EdB4du4LPQYDmvx83FwakCzmYq3QWx3azk4qjjoJLa2";
    }

    private static string ComputeSEBComponentsHash()
    {
      string directoryName = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
      List<string> stringList = new List<string>()
      {
        Path.Combine(directoryName, "SafeExamBrowser.exe"),
        Path.Combine(directoryName, "SEBConfigTool.exe"),
        Path.Combine(directoryName, "Fleck.dll"),
        Path.Combine(directoryName, "IconLib.dll"),
        Path.Combine(directoryName, "MetroFramework.dll"),
        Path.Combine(directoryName, "SEBWindowsServiceContracts.dll"),
        Path.Combine(directoryName, "SebWindowsServiceWCF", "SebWindowsServiceWCF.exe"),
        Path.Combine(directoryName, "SebWindowsServiceWCF", "SEBWindowsServiceContracts.dll"),
        Path.Combine(directoryName, "SebWindowsServiceWCF", "Interop.WUApiLib.dll")
      };
      string path = Path.Combine(directoryName, "SebWindowsBrowser");
      if (Directory.Exists(path))
        stringList.AddRange((IEnumerable<string>) Directory.GetFiles(path, "*.*", SearchOption.AllDirectories));
      return SEBProtectionController.ComputeHashForFiles((IEnumerable<string>) stringList);
    }

    public static string ComputeHashForFiles(IEnumerable<string> fileNames)
    {
      StringBuilder stringBuilder = new StringBuilder();
      foreach (string fileName in fileNames)
        stringBuilder.Append(SEBProtectionController.ComputeFileHash(fileName));
      return BitConverter.ToString(new SHA256Managed().ComputeHash(Encoding.UTF8.GetBytes(stringBuilder.ToString()))).Replace("-", string.Empty);
    }

    private static string ComputeFileHash(string file)
    {
      if (!File.Exists(file))
        return (string) null;
      using (FileStream fileStream = File.OpenRead(file))
        return BitConverter.ToString(new SHA256Managed().ComputeHash((Stream) fileStream)).Replace("-", string.Empty);
    }

    public static byte[] GenerateBrowserExamKeySalt()
    {
      return AESThenHMAC.NewKey();
    }

    private enum EncryptionT
    {
      pkhs,
      pswd,
      plnd,
      pwcc,
      unknown,
    }
  }
}
