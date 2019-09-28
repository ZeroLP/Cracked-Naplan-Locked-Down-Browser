
using SebWindowsClient.DesktopUtils;
using SebWindowsClient.DiagnosticsUtils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace SebWindowsClient.ConfigurationUtils
{
  public class SEBClientInfo
  {
    public static bool examMode = false;
    public static string LoadingSettingsFileName = "";
    public static float scaleFactor = 1f;
    public static int appChooserHeight = 132;
    public const string SEB_CLIENT_CONFIG = "SebClientSettings.seb";
    public const string SEB_CLIENT_LOG = "SebClient.log";
    private const string XUL_RUNNER_CONFIG = "config.json";
    public const string XUL_RUNNER = "xulrunner.exe";
    private const string XUL_RUNNER_INI = "seb.ini";
    public const string MANUFACTURER_LOCAL = "SafeExamBrowser";
    public const string PRODUCT_NAME = "SafeExamBrowser";
    public const string SEB_SERVICE_DIRECTORY = "SebWindowsServiceWCF";
    public const string SEB_BROWSER_DIRECTORY = "SebWindowsBrowser";
    public const string SEB_RESOURCE_DIRECTORY = "SebWindowsResources";
    private const string XUL_RUNNER_DIRECTORY = "xulrunner";
    private const string XUL_SEB_DIRECTORY = "xul_seb";
    public const string FILENAME_SEB = "SafeExamBrowser.exe";
    public const string FILENAME_SEBCONFIGTOOL = "SEBConfigTool.exe";
    public const string FILENAME_SEBSERVICE = "SebWindowsServiceWCF.exe";
    public const string FILENAME_DLL_FLECK = "Fleck.dll";
    public const string FILENAME_DLL_ICONLIB = "IconLib.dll";
    public const string FILENAME_DLL_METRO = "MetroFramework.dll";
    public const string FILENAME_DLL_SERVICECONTRACTS = "SEBWindowsServiceContracts.dll";
    public const string FILENAME_DLL_INTEROP = "Interop.WUApiLib.dll";
    public const string BROWSER_USERAGENT_DESKTOP = "Mozilla/5.0 (Windows NT {0}.{1}; rv:41.0) Gecko/20100101 Firefox/41";
    public const string BROWSER_USERAGENT_TOUCH = "Mozilla/5.0 (Windows NT 6.3; rv:41.0; Touch) Gecko/20100101 Firefox/41";
    public const string BROWSER_USERAGENT_TOUCH_IPAD = "Mozilla/5.0 (iPad; CPU OS 9_0_2 like Mac OS X) AppleWebKit/601.1.46 (KHTML, like Gecko) Version/9.0 Mobile/13A452 Safari/601.1";
    public const string BROWSER_USERAGENT_SEB = "SEB";
    public const string END_OF_STRING_KEYWORD = "---SEB---";
    private const string DEFAULT_USERNAME = "";
    private const string DEFAULT_HOSTNAME = "localhost";
    private const string DEFAULT_HOST_IP_ADDRESS = "127.0.0.1";
    private const int DEFAULT_PORTNUMBER = 57016;
    public const string DEFAULT_KEY = "Di\xD834\xDE2Dl\xD834\xDE16Ch\xD834\xDE12ah\xD834\xDE47t\xD834\xDE01a\xD834\xDE48Hai1972";
    private const int DEFAULT_SEND_INTERVAL = 100;
    private const int DEFAULT_RECV_TIMEOUT = 100;
    private const int DEFAULT_NUM_MESSAGES = 3;
    public const string SEB_NEW_DESKTOP_NAME = "SEBDesktop";
    public const string SEB_WINDOWS_SERVICE_NAME = "SebWindowsService";
    public const string SEB_RESOURCE_DIRECTORY_URL = "$(ResourceDirectory)";
    public static string SebClientSettingsProgramDataFile;
    public static string SebClientSettingsAppDataFile;
    public static string XulRunnerConfigFile;
    public static string XulRunnerExePath;
    public static string XulRunnerSebIniPath;
    public static string XulRunnerParameter;
    public static SebWindowsClientForm SebWindowsClientForm;

    [DllImport("kernel32.Dll")]
    public static extern short GetVersionEx(ref SEBClientInfo.OSVERSIONINFO o);

    public static bool ExplorerShellWasKilled { get; set; }

    public static bool IsNewOS { get; set; }

    public static char[] UserNameRegistryFlags { get; set; }

    public static char[] RegistryFlags { get; set; }

    public static string HostName { get; set; }

    public static string HostIpAddress { get; set; }

    public static string UserName { get; set; }

    public static char[] UserSid { get; set; }

    public static int PortNumber { get; set; }

    public static int SendInterval { get; set; }

    public static int RecvTimeout { get; set; }

    public static int NumMessages { get; set; }

    public static int MessageNr { get; set; }

    public static SEBDesktopController OriginalDesktop { get; set; }

    public static SEBDesktopController SEBNewlDesktop { get; set; }

    public static string DesktopName { get; set; }

    public static string ApplicationExecutableDirectory { get; set; }

    public static string ProgramFilesX86Directory { get; set; }

    public static bool LogFileDesiredMsgHook { get; set; }

    public static bool LogFileDesiredSebClient { get; set; }

    public static string SebClientLogFileDirectory { get; set; }

    public static string SebClientDirectory { get; set; }

    public static string SebClientLogFile { get; set; }

    public static string SebClientSettingsProgramDataDirectory { get; set; }

    public static string SebClientSettingsAppDataDirectory { get; set; }

    public static string XulRunnerDirectory { get; set; }

    public static string XulSebDirectory { get; set; }

    public static string XulRunnerConfigFileDirectory { get; set; }

    public static string SebResourceDirectory { get; set; }

    public static string ExamUrl { get; set; }

    public static string QuitPassword { get; set; }

    public static string QuitHashcode { get; set; }

    public static Dictionary<string, object> getSebSetting(string key)
    {
      object obj;
      try
      {
        obj = SEBSettings.settingsCurrent[key];
      }
      catch
      {
        obj = (object) null;
      }
      if (obj != null)
        return SEBSettings.settingsCurrent;
      return SEBSettings.settingsDefault;
    }

    public static bool SetSebClientConfiguration()
    {
      SEBClientInfo.IsNewOS = false;
      SEBClientInfo.ExplorerShellWasKilled = false;
      SEBClientInfo.UserNameRegistryFlags = new char[100];
      SEBClientInfo.RegistryFlags = new char[50];
      SEBClientInfo.UserSid = new char[512];
      SEBClientInfo.UserName = "";
      SEBClientInfo.HostName = "localhost";
      SEBClientInfo.HostIpAddress = "127.0.0.1";
      SEBClientInfo.PortNumber = 57016;
      SEBClientInfo.SendInterval = 100;
      SEBClientInfo.RecvTimeout = 100;
      SEBClientInfo.NumMessages = 3;
      SEBClientInfo.SetSebPaths();
      byte[] sebSettings = (byte[]) null;
      StringBuilder stringBuilder = new StringBuilder();
      try
      {
        sebSettings = File.ReadAllBytes(SEBClientInfo.SebClientSettingsProgramDataFile);
      }
      catch (Exception ex)
      {
        stringBuilder.Append("Could not load SebClientSettigs.seb from the Program Data directory").Append(ex == null ? (string) null : ex.GetType().ToString()).Append(ex.Message);
      }
      if (sebSettings == null)
      {
        try
        {
          sebSettings = File.ReadAllBytes(SEBClientInfo.SebClientSettingsAppDataFile);
        }
        catch (Exception ex)
        {
          stringBuilder.Append("Could not load SebClientSettigs.seb from the Roaming Application Data directory. ").Append(ex == null ? (string) null : ex.GetType().ToString()).Append(ex.Message);
        }
      }
      if (!SEBSettings.StoreDecryptedSebClientSettings(sebSettings))
        return false;
      SEBClientInfo.InitializeLogger();
      Logger.AddError(stringBuilder.ToString(), (object) null, (Exception) null, (string) null);
      SEBClientInfo.UserName = Environment.UserName;
      int num = 1;
      Logger.AddInformation(new StringBuilder("User Name: ").Append(SEBClientInfo.UserName).Append(" Host Name: ").Append(SEBClientInfo.HostName).Append(" Port Number: ").Append(SEBClientInfo.PortNumber).Append(" Send Interval: ").Append(SEBClientInfo.SendInterval).Append(" Recv Timeout: ").Append(SEBClientInfo.RecvTimeout).Append(" Num Messages: ").Append(SEBClientInfo.NumMessages).Append(" SebClientConfigFileDirectory: ").Append(SEBClientInfo.SebClientSettingsAppDataDirectory).Append(" SebClientConfigFile: ").Append(SEBClientInfo.SebClientSettingsAppDataFile).ToString(), (object) null, (Exception) null, (string) null);
      return num != 0;
    }

    public static void InitializeLogger()
    {
      if (!(bool) SEBClientInfo.getSebSetting("enableLogging")["enableLogging"])
        return;
      string name = (string) SEBSettings.valueForDictionaryKey(SEBSettings.settingsCurrent, "logDirectoryWin");
      if (!string.IsNullOrEmpty(name))
      {
        SEBClientInfo.SebClientLogFileDirectory = Environment.ExpandEnvironmentVariables(name);
        SEBClientInfo.SebClientLogFile = string.Format("{0}\\{1}", (object) SEBClientInfo.SebClientLogFileDirectory, (object) "SebClient.log");
      }
      else
        SEBClientInfo.SetDefaultClientLogFile();
      Logger.InitLogger(SEBClientInfo.SebClientLogFileDirectory, (string) null);
    }

    public static void SetSebPaths()
    {
      SEBClientInfo.ApplicationExecutableDirectory = Path.GetDirectoryName(Application.ExecutablePath);
      SEBClientInfo.ProgramFilesX86Directory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
      string folderPath1 = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
      string folderPath2 = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
      SEBClientInfo.SebClientSettingsProgramDataDirectory = new StringBuilder(folderPath1).Append("\\").Append("SafeExamBrowser").Append("\\").ToString();
      SEBClientInfo.SebClientSettingsAppDataDirectory = new StringBuilder(folderPath2).Append("\\").Append("SafeExamBrowser").Append("\\").ToString();
      SEBClientInfo.SebClientDirectory = new StringBuilder(SEBClientInfo.ProgramFilesX86Directory).Append("\\").Append("SafeExamBrowser").Append("\\").ToString();
      SEBClientInfo.XulRunnerDirectory = new StringBuilder("SebWindowsBrowser").Append("\\").Append("xulrunner").Append("\\").ToString();
      SEBClientInfo.XulSebDirectory = new StringBuilder("SebWindowsBrowser").Append("\\").Append("xul_seb").Append("\\").ToString();
      SEBClientInfo.XulRunnerExePath = new StringBuilder(SEBClientInfo.XulRunnerDirectory).Append("xulrunner.exe").ToString();
      SEBClientInfo.XulRunnerSebIniPath = new StringBuilder(SEBClientInfo.XulSebDirectory).Append("seb.ini").ToString();
      SEBClientInfo.SebClientSettingsProgramDataFile = new StringBuilder(SEBClientInfo.SebClientSettingsProgramDataDirectory).Append("SebClientSettings.seb").ToString();
      SEBClientInfo.SebClientSettingsAppDataFile = new StringBuilder(SEBClientInfo.SebClientSettingsAppDataDirectory).Append("SebClientSettings.seb").ToString();
      SEBClientInfo.SebResourceDirectory = new StringBuilder(SEBClientInfo.ApplicationExecutableDirectory).Append("\\").Append("SebWindowsResources").Append("\\").ToString();
      SEBClientInfo.SetDefaultClientLogFile();
    }

    public static void SetDefaultClientLogFile()
    {
      SEBClientInfo.SebClientLogFileDirectory = new StringBuilder(SEBClientInfo.SebClientSettingsAppDataDirectory).ToString();
      SEBClientInfo.SebClientLogFile = new StringBuilder(SEBClientInfo.SebClientLogFileDirectory).Append("SebClient.log").ToString();
    }

    public static bool SetXulRunnerConfiguration()
    {
      bool flag = false;
      try
      {
        SEBClientInfo.XulRunnerConfigFileDirectory = new StringBuilder(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)).Append("\\").Append("SafeExamBrowser").Append("\\").ToString();
        SEBClientInfo.XulRunnerConfigFile = new StringBuilder(SEBClientInfo.XulRunnerConfigFileDirectory).Append("config.json").ToString();
        XULRunnerConfig objXULRunnerConfig = SEBXulRunnerSettings.XULRunnerConfigDeserialize(SEBClientInfo.XulRunnerConfigFile);
        objXULRunnerConfig.seb_openwin_width = int.Parse(SEBClientInfo.getSebSetting("newBrowserWindowByLinkWidth")["newBrowserWindowByLinkWidth"].ToString());
        objXULRunnerConfig.seb_openwin_height = int.Parse(SEBClientInfo.getSebSetting("newBrowserWindowByLinkHeight")["newBrowserWindowByLinkHeight"].ToString());
        objXULRunnerConfig.seb_mainWindow_titlebar_enabled = (int) SEBClientInfo.getSebSetting("browserViewMode")["browserViewMode"] == 0;
        objXULRunnerConfig.seb_url = SEBClientInfo.getSebSetting("startURL")["startURL"].ToString();
        flag = true;
        SEBXulRunnerSettings.XULRunnerConfigSerialize(objXULRunnerConfig, SEBClientInfo.XulRunnerConfigFile);
      }
      catch (Exception ex)
      {
        Logger.AddError("Error ocurred by setting XulRunner configuration.", (object) null, ex, ex.Message);
      }
      return flag;
    }

    public static bool SetSystemVersionInfo()
    {
      SEBClientInfo.OSVERSIONINFO o = new SEBClientInfo.OSVERSIONINFO();
      o.dwOSVersionInfoSize = Marshal.SizeOf(typeof (SEBClientInfo.OSVERSIONINFO));
      int num = 800;
      try
      {
        if ((int) SEBClientInfo.GetVersionEx(ref o) != 0)
        {
          switch (o.dwPlatformId)
          {
            case 1:
              switch (o.dwMinorVersion)
              {
                case 0:
                  num = 950;
                  break;
                case 10:
                  num = 980;
                  break;
                case 90:
                  num = 999;
                  break;
                default:
                  num = 800;
                  break;
              }
                            break;
            case 2:
              switch (o.dwMajorVersion)
              {
                case 3:
                  num = 1351;
                  break;
                case 4:
                  num = 1400;
                  break;
                case 5:
                  num = o.dwMinorVersion != 0 ? 2010 : 2000;
                  break;
                case 6:
                  num = o.dwMinorVersion != 0 ? (o.dwMinorVersion != 1 ? (o.dwMinorVersion != 2 ? 2050 : 2050) : 2050) : 2050;
                  break;
                case 10:
                  num = 2050;
                  break;
                default:
                  num = 800;
                  break;
              }
                            break;
            default:
              num = 800;
              break;
          }
        }
      }
      catch (Exception ex)
      {
        Logger.AddError("SetSystemVersionInfo.", (object) null, ex, (string) null);
      }
      Logger.AddInformation("OS Version: " + num.ToString(), (object) null, (Exception) null, (string) null);
      if (num <= 1351)
      {
        if (num <= 980)
        {
          if (num != 950 && num != 980)
            goto label_27;
        }
        else if (num != 999)
        {
          if (num == 1351)
            goto label_25;
          else
            goto label_27;
        }
        SEBClientInfo.IsNewOS = false;
        return true;
      }
      if (num <= 2000)
      {
        if (num != 1400 && num != 2000)
          goto label_27;
      }
      else if (num != 2010 && num != 2050)
        goto label_27;
label_25:
      SEBClientInfo.IsNewOS = true;
      return true;
label_27:
      return false;
    }

    public static bool CreateNewDesktopOldValue { get; set; }

    public static string ContractEnvironmentVariables(string path)
    {
      path = Path.GetFullPath(path);
      DictionaryEntry dictionaryEntry = new DictionaryEntry((object) "", (object) "");
      foreach (object key in (IEnumerable) Environment.GetEnvironmentVariables().Keys)
      {
        string environmentVariable = (string) Environment.GetEnvironmentVariables()[key];
        if (path.ToUpperInvariant().Contains(environmentVariable.ToUpperInvariant()) && environmentVariable.Length > ((string) dictionaryEntry.Value).Length)
        {
          dictionaryEntry.Key = (object) (string) key;
          dictionaryEntry.Value = (object) environmentVariable;
        }
      }
      return path.Replace((string) dictionaryEntry.Value, "%" + (string) dictionaryEntry.Key + "%");
    }

    public struct OSVERSIONINFO
    {
      public int dwOSVersionInfoSize;
      public int dwMajorVersion;
      public int dwMinorVersion;
      public int dwBuildNumber;
      public int dwPlatformId;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
      public string szCSDVersion;
    }
  }
}
