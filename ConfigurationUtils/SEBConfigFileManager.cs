

using PlistCS;
using SebWindowsClient.CryptographyUtils;
using SebWindowsClient.DiagnosticsUtils;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Windows.Forms;

namespace SebWindowsClient.ConfigurationUtils
{
  public class SEBConfigFileManager
  {
    public static SebPasswordDialogForm sebPasswordDialogForm;
    private const int PREFIX_LENGTH = 4;
    private const string PUBLIC_KEY_HASH_MODE = "pkhs";
    private const string PASSWORD_MODE = "pswd";
    private const string PLAIN_DATA_MODE = "plnd";
    private const string PASSWORD_CONFIGURING_CLIENT_MODE = "pwcc";
    private const string UNENCRYPTED_MODE = "<?xm";
    private const int PUBLIC_KEY_HASH_LENGTH = 20;

    public static bool StoreDecryptedSEBSettings(byte[] sebData)
    {
      string sebFilePassword = (string) null;
      bool passwordIsHash = false;
      X509Certificate2 sebFileCertificateRef = (X509Certificate2) null;
      Dictionary<string, object> settingsDict = SEBConfigFileManager.DecryptSEBSettings(sebData, false, ref sebFilePassword, ref passwordIsHash, ref sebFileCertificateRef);
      if (settingsDict == null)
        return false;
      Logger.AddInformation("Reconfiguring", (object) null, (Exception) null, (string) null);
      SEBClientInfo.SebWindowsClientForm.closeSebClient = false;
      Logger.AddInformation("Attempting to CloseSEBForm for reconfiguration", (object) null, (Exception) null, (string) null);
      SEBClientInfo.SebWindowsClientForm.CloseSEBForm();
      Logger.AddInformation("Succesfully CloseSEBForm for reconfiguration", (object) null, (Exception) null, (string) null);
      SEBClientInfo.SebWindowsClientForm.closeSebClient = true;
      SEBClientInfo.CreateNewDesktopOldValue = (bool) SEBSettings.valueForDictionaryKey(SEBSettings.settingsCurrent, "createNewDesktop");
      if ((int) settingsDict["sebConfigPurpose"] == 0)
      {
        Logger.AddInformation("Reconfiguring to start an exam", (object) null, (Exception) null, (string) null);
        Logger.AddInformation("Attempting to StoreSebClientSettings", (object) null, (Exception) null, (string) null);
        SEBSettings.StoreSebClientSettings(settingsDict);
        Logger.AddInformation("Successfully StoreSebClientSettings", (object) null, (Exception) null, (string) null);
        SEBClientInfo.examMode = true;
        SEBClientInfo.InitializeLogger();
        if (SEBClientInfo.CreateNewDesktopOldValue != (bool) SEBSettings.valueForDictionaryKey(SEBSettings.settingsCurrent, "createNewDesktop"))
        {
          if (!SEBClientInfo.CreateNewDesktopOldValue)
          {
            int num1 = (int) SEBMessageBox.Show(SEBUIStrings.settingsRequireNewDesktop, SEBUIStrings.settingsRequireNewDesktopReason, MessageBoxIcon.Hand, MessageBoxButtons.OK, false);
          }
          else
          {
            int num2 = (int) SEBMessageBox.Show(SEBUIStrings.settingsRequireNotNewDesktop, SEBUIStrings.settingsRequireNotNewDesktopReason, MessageBoxIcon.Hand, MessageBoxButtons.OK, false);
          }
          SEBClientInfo.SebWindowsClientForm.ExitApplication(true);
        }
        Logger.AddInformation("Attemting to InitSEBDesktop for reconfiguration", (object) null, (Exception) null, (string) null);
        if (!SebWindowsClientMain.InitSEBDesktop())
          return false;
        Logger.AddInformation("Sucessfully InitSEBDesktop for reconfiguration", (object) null, (Exception) null, (string) null);
        Logger.AddInformation("Attempting to OpenSEBForm for reconfiguration", (object) null, (Exception) null, (string) null);
        int num = SEBClientInfo.SebWindowsClientForm.OpenSEBForm() ? 1 : 0;
        Logger.AddInformation("Successfully OpenSEBForm for reconfiguration", (object) null, (Exception) null, (string) null);
        return num != 0;
      }
      Logger.AddInformation("Reconfiguring to configure a client", (object) null, (Exception) null, (string) null);
      List<object> objectList = (List<object>) settingsDict["embeddedCertificates"];
      for (int index = objectList.Count - 1; index >= 0; --index)
      {
        Dictionary<string, object> dictionary = (Dictionary<string, object>) objectList[index];
        if ((int) dictionary["type"] == 1)
          SEBProtectionController.StoreCertificateIntoStore((byte[]) dictionary["certificateData"]);
        objectList.RemoveAt(index);
      }
      SEBSettings.StoreSebClientSettings(settingsDict);
      SEBClientInfo.InitializeLogger();
      SEBSettings.WriteSebConfigurationFile(SEBClientInfo.SebClientSettingsAppDataFile, "", false, (X509Certificate2) null, SEBSettings.sebConfigPurposes.sebConfigPurposeConfiguringClient, false);
      if (!SebWindowsClientMain.InitSEBDesktop() || !SEBClientInfo.SebWindowsClientForm.OpenSEBForm())
        return false;
      if (SEBClientInfo.CreateNewDesktopOldValue != (bool) SEBSettings.valueForDictionaryKey(SEBSettings.settingsCurrent, "createNewDesktop"))
      {
        int num = (int) SEBMessageBox.Show(SEBUIStrings.sebReconfiguredRestartNeeded, SEBUIStrings.sebReconfiguredRestartNeededReason, MessageBoxIcon.Exclamation, MessageBoxButtons.OK, false);
        SEBClientInfo.SebWindowsClientForm.ExitApplication(true);
      }
      if (SEBMessageBox.Show(SEBUIStrings.sebReconfigured, SEBUIStrings.sebReconfiguredQuestion, MessageBoxIcon.Question, MessageBoxButtons.YesNo, false) == DialogResult.No)
        SEBClientInfo.SebWindowsClientForm.ExitApplication(true);
      return true;
    }

    public static Dictionary<string, object> DecryptSEBSettings(byte[] sebData, bool forEditing, ref string sebFilePassword, ref bool passwordIsHash, ref X509Certificate2 sebFileCertificateRef)
    {
      byte[] numArray1 = GZipByte.Decompress(sebData);
      if (numArray1 != null)
        sebData = numArray1;
      byte[] numArray2 = sebData.Clone() as byte[];
      string prefixStringFromData = SEBConfigFileManager.GetPrefixStringFromData(ref sebData);
      if (prefixStringFromData.CompareTo("pkhs") == 0)
      {
        sebData = SEBConfigFileManager.DecryptDataWithPublicKeyHashPrefix(sebData, forEditing, ref sebFileCertificateRef);
        if (sebData == null)
          return (Dictionary<string, object>) null;
        prefixStringFromData = SEBConfigFileManager.GetPrefixStringFromData(ref sebData);
      }
      if (prefixStringFromData.CompareTo("pswd") == 0)
      {
        string passwordRequestText = SEBUIStrings.enterPassword;
        int num1 = 5;
        string passphrase;
        byte[] numArray3;
        do
        {
          --num1;
          passphrase = ThreadedDialog.ShowPasswordDialogForm(SEBUIStrings.loadingSettings, passwordRequestText);
          if (passphrase == null)
            return (Dictionary<string, object>) null;
          numArray3 = SEBProtectionController.DecryptDataWithPassword(sebData, passphrase);
          passwordRequestText = SEBUIStrings.enterPasswordAgain;
        }
        while (numArray3 == null && num1 > 0);
        if (numArray3 == null)
        {
          int num2 = (int) SEBMessageBox.Show(SEBUIStrings.decryptingSettingsFailed, SEBUIStrings.decryptingSettingsFailedReason, MessageBoxIcon.Hand, MessageBoxButtons.OK, forEditing);
          return (Dictionary<string, object>) null;
        }
        sebData = numArray3;
        if (forEditing)
          sebFilePassword = passphrase;
      }
      else
      {
        if (prefixStringFromData.CompareTo("pwcc") == 0)
          return SEBConfigFileManager.DecryptDataWithPasswordForConfiguringClient(sebData, forEditing, ref sebFilePassword, ref passwordIsHash);
        if (prefixStringFromData.CompareTo("plnd") != 0)
        {
          if (prefixStringFromData.CompareTo("<?xm") == 0)
          {
            sebData = numArray2;
          }
          else
          {
            int num = (int) SEBMessageBox.Show(SEBUIStrings.settingsNotUsable, SEBUIStrings.settingsNotUsableReason, MessageBoxIcon.Hand, MessageBoxButtons.OK, forEditing);
            return (Dictionary<string, object>) null;
          }
        }
      }
      if (prefixStringFromData.CompareTo("<?xm") != 0)
        sebData = GZipByte.Decompress(sebData);
      Dictionary<string, object> dictFromConfigData = SEBConfigFileManager.GetPreferencesDictFromConfigData(sebData, forEditing);
      if (dictFromConfigData == null)
        return (Dictionary<string, object>) null;
      dictFromConfigData["sebConfigPurpose"] = (object) 0;
      return dictFromConfigData;
    }

    private static Dictionary<string, object> DecryptDataWithPasswordForConfiguringClient(byte[] sebData, bool forEditing, ref string sebFilePassword, ref bool passwordIsHash)
    {
      passwordIsHash = false;
      string upper = ((string) SEBSettings.valueForDictionaryKey(SEBSettings.settingsCurrent, "hashedAdminPassword") ?? "").ToUpper();
      byte[] input1 = SEBProtectionController.DecryptDataWithPassword(sebData, upper);
      string str1;
      if (input1 == null)
      {
        input1 = SEBProtectionController.DecryptDataWithPassword(sebData, "");
        if (input1 == null)
        {
          int num1 = 5;
          str1 = (string) null;
          string passwordRequestText = SEBUIStrings.enterEncryptionPassword;
          string input2;
          do
          {
            --num1;
            input2 = ThreadedDialog.ShowPasswordDialogForm(SEBUIStrings.reconfiguringLocalSettings, passwordRequestText);
            if (input2 == null)
              return (Dictionary<string, object>) null;
            string passwordHash = SEBProtectionController.ComputePasswordHash(input2);
            input1 = SEBProtectionController.DecryptDataWithPassword(sebData, passwordHash);
            passwordRequestText = SEBUIStrings.enterEncryptionPasswordAgain;
          }
          while (input1 == null && num1 > 0);
          if (input1 == null)
          {
            int num2 = (int) SEBMessageBox.Show(SEBUIStrings.reconfiguringLocalSettingsFailed, SEBUIStrings.reconfiguringLocalSettingsFailedWrongPassword, MessageBoxIcon.Hand, MessageBoxButtons.OK, forEditing);
            return (Dictionary<string, object>) null;
          }
          if (forEditing)
            sebFilePassword = input2;
        }
      }
      else
      {
        sebFilePassword = upper;
        passwordIsHash = true;
      }
      byte[] data = GZipByte.Decompress(input1);
      Dictionary<string, object> dictionary;
      try
      {
        dictionary = (Dictionary<string, object>) Plist.readPlist(data);
      }
      catch (Exception ex)
      {
        int num = (int) SEBMessageBox.Show(SEBUIStrings.loadingSettingsFailed, SEBUIStrings.loadingSettingsFailedReason, MessageBoxIcon.Hand, MessageBoxButtons.OK, forEditing);
        Console.WriteLine(ex.Message);
        return (Dictionary<string, object>) null;
      }
      string str2 = (string) SEBSettings.valueForDictionaryKey(dictionary, "hashedAdminPassword") ?? "";
      if (string.Compare(upper, str2, StringComparison.OrdinalIgnoreCase) != 0)
      {
        if (forEditing)
        {
          if (!SEBConfigFileManager.askForPasswordAndCompareToHashedPassword(str2, forEditing))
            return (Dictionary<string, object>) null;
        }
        else if (!passwordIsHash && upper.Length > 0)
        {
          int num1 = 5;
          str1 = (string) null;
          string passwordRequestText = SEBUIStrings.enterCurrentAdminPwdForReconfiguring;
          bool flag;
          do
          {
            --num1;
            string input2 = ThreadedDialog.ShowPasswordDialogForm(SEBUIStrings.reconfiguringLocalSettings, passwordRequestText);
            if (input2 == null)
              return (Dictionary<string, object>) null;
            flag = string.Compare(input2.Length != 0 ? SEBProtectionController.ComputePasswordHash(input2) : "", upper, StringComparison.OrdinalIgnoreCase) == 0;
            passwordRequestText = SEBUIStrings.enterCurrentAdminPwdForReconfiguringAgain;
          }
          while (!flag && num1 > 0);
          if (!flag)
          {
            int num2 = (int) SEBMessageBox.Show(SEBUIStrings.reconfiguringLocalSettingsFailed, SEBUIStrings.reconfiguringLocalSettingsFailedWrongCurrentAdminPwd, MessageBoxIcon.Hand, MessageBoxButtons.OK, forEditing);
            return (Dictionary<string, object>) null;
          }
        }
      }
      dictionary["sebConfigPurpose"] = (object) 1;
      return dictionary;
    }

    private static Dictionary<string, object> GetPreferencesDictFromConfigData(byte[] sebData, bool forEditing)
    {
      Dictionary<string, object> dictionary;
      try
      {
        dictionary = (Dictionary<string, object>) Plist.readPlist(sebData);
      }
      catch (Exception ex)
      {
        int num = (int) SEBMessageBox.Show(SEBUIStrings.loadingSettingsFailed, SEBUIStrings.loadingSettingsFailedReason, MessageBoxIcon.Hand, MessageBoxButtons.OK, forEditing);
        Console.WriteLine(ex.Message);
        return (Dictionary<string, object>) null;
      }
      if (forEditing)
      {
        string str = (string) SEBSettings.valueForDictionaryKey(dictionary, "hashedAdminPassword");
        if (!string.IsNullOrEmpty(str) && (string.Compare((string) SEBSettings.valueForDictionaryKey(SEBSettings.settingsCurrent, "hashedAdminPassword") ?? "", str, StringComparison.OrdinalIgnoreCase) != 0 && !SEBConfigFileManager.askForPasswordAndCompareToHashedPassword(str, forEditing)))
          return (Dictionary<string, object>) null;
      }
      return dictionary;
    }

    private static bool askForPasswordAndCompareToHashedPassword(string sebFileHashedAdminPassword, bool forEditing)
    {
      if (sebFileHashedAdminPassword.Length == 0)
        return true;
      int num1 = 5;
      string passwordRequestText = SEBUIStrings.enterAdminPasswordRequired;
      string input;
      bool flag;
      do
      {
        --num1;
        input = ThreadedDialog.ShowPasswordDialogForm(SEBUIStrings.loadingSettings + (string.IsNullOrEmpty(SEBClientInfo.LoadingSettingsFileName) ? "" : ": " + SEBClientInfo.LoadingSettingsFileName), passwordRequestText);
        if (input == null)
          return false;
        flag = string.Compare(input.Length != 0 ? SEBProtectionController.ComputePasswordHash(input) : "", sebFileHashedAdminPassword, StringComparison.OrdinalIgnoreCase) == 0;
        passwordRequestText = SEBUIStrings.enterAdminPasswordRequiredAgain;
      }
      while ((input == null || !flag) && num1 > 0);
      if (flag)
        return flag;
      int num2 = (int) SEBMessageBox.Show(SEBUIStrings.loadingSettingsFailed, SEBUIStrings.loadingSettingsFailedWrongAdminPwd, MessageBoxIcon.Hand, MessageBoxButtons.OK, forEditing);
      return false;
    }

    private static byte[] DecryptDataWithPublicKeyHashPrefix(byte[] sebData, bool forEditing, ref X509Certificate2 sebFileCertificateRef)
    {
      X509Certificate2 certificateFromStore = SEBProtectionController.GetCertificateFromStore(SEBConfigFileManager.GetPrefixDataFromData(ref sebData, 20));
      if (certificateFromStore == null)
      {
        int num = (int) SEBMessageBox.Show(SEBUIStrings.errorDecryptingSettings, SEBUIStrings.certificateNotFoundInStore, MessageBoxIcon.Hand, MessageBoxButtons.OK, forEditing);
        return (byte[]) null;
      }
      if (forEditing)
        sebFileCertificateRef = certificateFromStore;
      sebData = SEBProtectionController.DecryptDataWithCertificate(sebData, certificateFromStore);
      return sebData;
    }

    public static string GetPrefixStringFromData(ref byte[] data)
    {
      return Encoding.UTF8.GetString(SEBConfigFileManager.GetPrefixDataFromData(ref data, 4));
    }

    public static byte[] GetPrefixDataFromData(ref byte[] data, int prefixLength)
    {
      byte[] numArray1 = new byte[prefixLength];
      Buffer.BlockCopy((Array) data, 0, (Array) numArray1, 0, prefixLength);
      byte[] numArray2 = new byte[data.Length - prefixLength];
      Buffer.BlockCopy((Array) data, prefixLength, (Array) numArray2, 0, data.Length - prefixLength);
      data = numArray2;
      return numArray1;
    }

    public static byte[] EncryptSEBSettingsWithCredentials(string settingsPassword, bool passwordIsHash, X509Certificate2 certificateRef, SEBSettings.sebConfigPurposes configPurpose, bool forEditing)
    {
      byte[] bytes = Encoding.UTF8.GetBytes(Plist.writeXml((object) SEBSettings.settingsCurrent).Replace("<array />", "<array></array>").Replace("<dict />", "<dict></dict>").Replace("<data />", "<data></data>"));
      string password = (string) null;
      if (string.IsNullOrEmpty(settingsPassword) && configPurpose == SEBSettings.sebConfigPurposes.sebConfigPurposeConfiguringClient)
        password = "";
      else if (string.IsNullOrEmpty(settingsPassword) && certificateRef == null)
      {
        if (SEBMessageBox.Show(SEBUIStrings.noEncryptionChosen, SEBUIStrings.noEncryptionChosenSaveUnencrypted, MessageBoxIcon.Question, MessageBoxButtons.YesNo, forEditing) == DialogResult.Yes)
          return bytes;
        return (byte[]) null;
      }
      byte[] data = GZipByte.Compress(bytes);
      if (!string.IsNullOrEmpty(settingsPassword))
        password = settingsPassword;
      byte[] numArray1;
      if (password != null)
      {
        numArray1 = SEBConfigFileManager.EncryptDataUsingPassword(data, password, passwordIsHash, configPurpose);
      }
      else
      {
        byte[] numArray2 = new byte[data.Length + 4];
        Buffer.BlockCopy((Array) Encoding.UTF8.GetBytes("plnd"), 0, (Array) numArray2, 0, 4);
        Buffer.BlockCopy((Array) data, 0, (Array) numArray2, 4, data.Length);
        numArray1 = (byte[]) numArray2.Clone();
      }
      if (certificateRef != null)
        numArray1 = SEBConfigFileManager.EncryptDataUsingIdentity(numArray1, certificateRef);
      return GZipByte.Compress(numArray1);
    }

    public static byte[] EncryptDataUsingIdentity(byte[] data, X509Certificate2 certificateRef)
    {
      byte[] hashFromCertificate = SEBProtectionController.GetPublicKeyHashFromCertificate(certificateRef);
      byte[] numArray1 = SEBProtectionController.EncryptDataWithCertificate(data, certificateRef);
      byte[] numArray2 = new byte[numArray1.Length + 4 + hashFromCertificate.Length];
      Buffer.BlockCopy((Array) Encoding.UTF8.GetBytes("pkhs"), 0, (Array) numArray2, 0, 4);
      Buffer.BlockCopy((Array) hashFromCertificate, 0, (Array) numArray2, 4, hashFromCertificate.Length);
      Buffer.BlockCopy((Array) numArray1, 0, (Array) numArray2, 4 + hashFromCertificate.Length, numArray1.Length);
      return numArray2;
    }

    public static byte[] EncryptDataUsingPassword(byte[] data, string password, bool passwordIsHash, SEBSettings.sebConfigPurposes configPurpose)
    {
      string s;
      if (configPurpose == SEBSettings.sebConfigPurposes.sebConfigPurposeStartingExam)
      {
        s = "pswd";
      }
      else
      {
        s = "pwcc";
        if (!string.IsNullOrEmpty(password) && !passwordIsHash)
          password = SEBProtectionController.ComputePasswordHash(password);
      }
      byte[] numArray1 = SEBProtectionController.EncryptDataWithPassword(data, password);
      byte[] numArray2 = new byte[numArray1.Length + 4];
      Buffer.BlockCopy((Array) Encoding.UTF8.GetBytes(s), 0, (Array) numArray2, 0, 4);
      Buffer.BlockCopy((Array) numArray1, 0, (Array) numArray2, 4, numArray1.Length);
      return numArray2;
    }
  }
}
