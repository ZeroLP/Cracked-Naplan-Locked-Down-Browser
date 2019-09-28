

using SebWindowsClient.CryptographyUtils;
using SebWindowsClient.DiagnosticsUtils;
using SebWindowsClient.XULRunnerCommunication;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace SebWindowsClient.ConfigurationUtils
{
  public class SEBXulRunnerSettings
  {
    public static void XULRunnerConfigSerialize(XULRunnerConfig objXULRunnerConfig, string path)
    {
      string str1 = new JavaScriptSerializer().Serialize((object) objXULRunnerConfig).Replace("_", ".");
      System.IO.File.Delete(path);
      FileStream fileStream = System.IO.File.Open(path, FileMode.CreateNew);
      StreamWriter streamWriter = new StreamWriter((Stream) fileStream);
      string str2 = str1;
      streamWriter.Write(str2);
      streamWriter.Close();
      fileStream.Close();
    }

    public static XULRunnerConfig XULRunnerConfigDeserialize(string path)
    {
      JavaScriptSerializer scriptSerializer = new JavaScriptSerializer();
      FileStream fileStream = System.IO.File.OpenRead(path);
      long num = 0;
      fileStream.Position = num;
      StreamReader streamReader = new StreamReader((Stream) fileStream);
      string end = streamReader.ReadToEnd();
      streamReader.Close();
      fileStream.Close();
      string input = end.Replace("\n", string.Empty).Replace("\r", string.Empty).Replace(" ", string.Empty).Replace("\t", string.Empty).Replace(".", "_");
      Type targetType = typeof (XULRunnerConfig);
      return (XULRunnerConfig) scriptSerializer.Deserialize(input, targetType);
    }

    public static string XULRunnerConfigDictionarySerialize(Dictionary<string, object> xulRunnerSettings)
    {
      if ((bool) xulRunnerSettings["sendBrowserExamKey"])
      {
        string browserExamKey = SEBProtectionController.ComputeBrowserExamKey();
        xulRunnerSettings["browserExamKey"] = (object) browserExamKey;
        xulRunnerSettings["browserURLSalt"] = (object) true;
      }
      xulRunnerSettings["startURL"] = (object) SEBXulRunnerSettings.ResolveResourceUrl((string) xulRunnerSettings["startURL"]);
      SEBXulRunnerSettings.OverwriteSystemProxySettings(xulRunnerSettings);
      if ((bool) SEBSettings.settingsCurrent["restartExamUseStartURL"])
        xulRunnerSettings["restartExamURL"] = xulRunnerSettings["startURL"];
      if (!(bool) xulRunnerSettings["URLFilterEnable"])
      {
        xulRunnerSettings["blacklistURLFilter"] = (object) "";
        xulRunnerSettings["whitelistURLFilter"] = (object) "";
      }
      else
      {
        xulRunnerSettings["urlFilterTrustedContent"] = (object) (bool) xulRunnerSettings["URLFilterEnableContentFilter"];
        if (!xulRunnerSettings["whitelistURLFilter"].ToString().Contains(xulRunnerSettings["startURL"].ToString()) && !string.IsNullOrWhiteSpace(xulRunnerSettings["whitelistURLFilter"].ToString()))
        {
          Dictionary<string, object> dictionary = xulRunnerSettings;
          dictionary["whitelistURLFilter"] = (object) (dictionary["whitelistURLFilter"].ToString() + ";");
        }
        Dictionary<string, object> dictionary1 = xulRunnerSettings;
        dictionary1["whitelistURLFilter"] = (object) (dictionary1["whitelistURLFilter"].ToString() + xulRunnerSettings["startURL"].ToString());
        if ((bool) xulRunnerSettings["URLFilterEnableContentFilter"])
        {
          if (!string.IsNullOrWhiteSpace(xulRunnerSettings["whitelistURLFilter"].ToString()))
          {
            Dictionary<string, object> dictionary2 = xulRunnerSettings;
            dictionary2["whitelistURLFilter"] = (object) (dictionary2["whitelistURLFilter"].ToString() + ";");
          }
          Dictionary<string, object> dictionary3 = xulRunnerSettings;
          dictionary3["whitelistURLFilter"] = (object) (dictionary3["whitelistURLFilter"].ToString() + string.Format("http://{0}", (object) SEBXULRunnerWebSocketServer.ServerAddress.Substring(5)));
        }
      }
      xulRunnerSettings["browserMessagingSocket"] = (object) SEBXULRunnerWebSocketServer.ServerAddress;
      Logger.AddInformation("Socket: " + xulRunnerSettings["browserMessagingSocket"].ToString(), (object) null, (Exception) null, (string) null);
      string str = Environment.ExpandEnvironmentVariables((string) xulRunnerSettings["downloadDirectoryWin"]);
      xulRunnerSettings["downloadDirectoryWin"] = (object) str;
      if ((bool) xulRunnerSettings["touchOptimized"])
      {
        xulRunnerSettings["showReloadWarning"] = (object) false;
        xulRunnerSettings["taskBarHeight"] = (object) (int) Math.Round((double) (int) xulRunnerSettings["taskBarHeight"] * 1.7);
        if ((int) xulRunnerSettings["browserUserAgentWinTouchMode"] == 0)
          xulRunnerSettings["browserUserAgent"] = (object) "Mozilla/5.0 (Windows NT 6.3; rv:41.0; Touch) Gecko/20100101 Firefox/41";
        else if ((int) xulRunnerSettings["browserUserAgentWinTouchMode"] == 1)
          xulRunnerSettings["browserUserAgent"] = (object) "Mozilla/5.0 (iPad; CPU OS 9_0_2 like Mac OS X) AppleWebKit/601.1.46 (KHTML, like Gecko) Version/9.0 Mobile/13A452 Safari/601.1";
        else if ((int) xulRunnerSettings["browserUserAgentWinTouchMode"] == 2)
          xulRunnerSettings["browserUserAgent"] = xulRunnerSettings["browserUserAgentWinTouchModeCustom"];
      }
      else if ((int) xulRunnerSettings["browserUserAgentWinDesktopMode"] == 0)
      {
        OperatingSystem osVersion = Environment.OSVersion;
        xulRunnerSettings["browserUserAgent"] = (object) string.Format("Mozilla/5.0 (Windows NT {0}.{1}; rv:41.0) Gecko/20100101 Firefox/41", (object) osVersion.Version.Major, (object) osVersion.Version.Minor);
      }
      else
        xulRunnerSettings["browserUserAgent"] = xulRunnerSettings["browserUserAgentWinDesktopModeCustom"];
      Dictionary<string, object> dictionary4 = xulRunnerSettings;
      dictionary4["browserUserAgent"] = (object) (dictionary4["browserUserAgent"].ToString() + " SEB " + Application.ProductVersion);
      xulRunnerSettings["browserScreenKeyboard"] = (object) (bool) xulRunnerSettings["touchOptimized"];
      return Convert.ToBase64String(Encoding.UTF8.GetBytes(new JavaScriptSerializer().Serialize((object) xulRunnerSettings)));
    }

    public static void OverwriteSystemProxySettings(Dictionary<string, object> currentSettings)
    {
      if ((int) currentSettings["proxySettingsPolicy"] == 1)
        return;
      IWebProxy systemWebProxy = WebRequest.GetSystemWebProxy();
      string remoteAddress1 = "http://www.janison.com.au";
      string keyEnable1 = "HTTPEnable";
      string keyHost1 = "HTTPProxy";
      string keyPort1 = "HTTPPort";
      bool flag1 = SEBXulRunnerSettings.UpdateProxySettings(systemWebProxy, remoteAddress1, keyEnable1, keyHost1, keyPort1);
      string remoteAddress2 = "https://www.janison.com.au";
      string keyEnable2 = "HTTPSEnable";
      string keyHost2 = "HTTPSProxy";
      string keyPort2 = "HTTPSPort";
      bool flag2 = SEBXulRunnerSettings.UpdateProxySettings(systemWebProxy, remoteAddress2, keyEnable2, keyHost2, keyPort2);
      if (flag1 | flag2)
        currentSettings["proxySettingsPolicy"] = (object) 1;
      else
        currentSettings["proxySettingsPolicy"] = (object) 0;
    }

    public static bool UpdateProxySettings(IWebProxy systemProxy, string remoteAddress, string keyEnable, string keyHost, string keyPort)
    {
      Uri uri = new Uri(remoteAddress);
      if (systemProxy.IsBypassed(uri))
      {
        SEBSettings.proxiesData[keyEnable] = (object) false;
        Logger.AddInformation(string.Format("Disabled proxy for {0}", (object) uri.Scheme), (object) null, (Exception) null, (string) null);
        return false;
      }
      Uri proxy = systemProxy.GetProxy(uri);
      Logger.AddInformation(string.Format("Enabled proxy for {0}: {1}:{2}", (object) uri.Scheme, (object) proxy.Host, (object) proxy.Port), (object) null, (Exception) null, (string) null);
      SEBSettings.proxiesData[keyEnable] = (object) true;
      SEBSettings.proxiesData[keyHost] = (object) proxy.Host;
      SEBSettings.proxiesData[keyPort] = (object) proxy.Port;
      return true;
    }

    public static string ResolveResourceUrl(string path)
    {
      return string.Format("file:///{0}", (object) SEBXulRunnerSettings.ResolveResourcePath(path));
    }

    public static string ResolveResourcePath(string path)
    {
      if (!path.StartsWith("$(ResourceDirectory)"))
        return path;
      return string.Format("{0}{1}", (object) SEBClientInfo.SebResourceDirectory.Replace('\\', '/'), (object) path.Substring("$(ResourceDirectory)".Length));
    }
  }
}
