

using SebWindowsClient.DiagnosticsUtils;
using SebWindowsClient.XULRunnerCommunication;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace SebWindowsClient.ConfigurationUtils
{
  public class SEBSettings
  {
    public static string[] strArrayDefault = new string[7];
    public static string[] strArrayCurrent = new string[7];
    public static int[] intArrayDefault = new int[7];
    public static int[] intArrayCurrent = new int[7];
    public static Dictionary<string, object> settingsDefault = new Dictionary<string, object>();
    public static Dictionary<string, object> settingsCurrent = new Dictionary<string, object>();
    public static List<object> permittedProcessList = new List<object>();
    public static Dictionary<string, object> permittedProcessData = new Dictionary<string, object>();
    public static Dictionary<string, object> permittedProcessDataDefault = new Dictionary<string, object>();
    public static Dictionary<string, object> permittedProcessDataXulRunner = new Dictionary<string, object>();
    public static List<object> permittedArgumentList = new List<object>();
    public static Dictionary<string, object> permittedArgumentData = new Dictionary<string, object>();
    public static Dictionary<string, object> permittedArgumentDataDefault = new Dictionary<string, object>();
    public static Dictionary<string, object> permittedArgumentDataXulRunner1 = new Dictionary<string, object>();
    public static Dictionary<string, object> permittedArgumentDataXulRunner2 = new Dictionary<string, object>();
    public static List<object> permittedArgumentListXulRunner = new List<object>();
    public static List<object> prohibitedProcessList = new List<object>();
    public static Dictionary<string, object> prohibitedProcessData = new Dictionary<string, object>();
    public static Dictionary<string, object> prohibitedProcessDataDefault = new Dictionary<string, object>();
    public static List<object> urlFilterRuleList = new List<object>();
    public static Dictionary<string, object> urlFilterRuleData = new Dictionary<string, object>();
    public static Dictionary<string, object> urlFilterRuleDataDefault = new Dictionary<string, object>();
    public static Dictionary<string, object> urlFilterRuleDataStorage = new Dictionary<string, object>();
    public static List<object> urlFilterActionList = new List<object>();
    public static List<object> urlFilterActionListDefault = new List<object>();
    public static List<object> urlFilterActionListStorage = new List<object>();
    public static Dictionary<string, object> urlFilterActionData = new Dictionary<string, object>();
    public static Dictionary<string, object> urlFilterActionDataDefault = new Dictionary<string, object>();
    public static Dictionary<string, object> urlFilterActionDataStorage = new Dictionary<string, object>();
    public static List<object> embeddedCertificateList = new List<object>();
    public static Dictionary<string, object> embeddedCertificateData = new Dictionary<string, object>();
    public static Dictionary<string, object> embeddedCertificateDataDefault = new Dictionary<string, object>();
    public static Dictionary<string, object> proxiesData = new Dictionary<string, object>();
    public static Dictionary<string, object> proxiesDataDefault = new Dictionary<string, object>();
    public static List<object> bypassedProxyList = new List<object>();
    public static string bypassedProxyData = "";
    public static string bypassedProxyDataDefault = "";
    public const string DefaultSebConfigXml = "SebClient.xml";
    public const string DefaultSebConfigSeb = "SebClient.seb";
    private const int IntOSX = 0;
    private const int IntWin = 1;
    public const int ValCryptoIdentity = 1;
    public const int ValMainBrowserWindowWidth = 2;
    public const int ValMainBrowserWindowHeight = 3;
    public const int ValNewBrowserWindowByLinkWidth = 4;
    public const int ValNewBrowserWindowByLinkHeight = 5;
    public const int ValTaskBarHeight = 6;
    public const int ValNum = 6;
    public const string KeyOriginatorVersion = "originatorVersion";
    public const string KeyStartURL = "startURL";
    public const string KeySebServerURL = "sebServerURL";
    public const string KeyHashedAdminPassword = "hashedAdminPassword";
    public const string KeyAllowQuit = "allowQuit";
    public const string KeyIgnoreExitKeys = "ignoreExitKeys";
    public const string KeyHashedQuitPassword = "hashedQuitPassword";
    public const string KeyExitKey1 = "exitKey1";
    public const string KeyExitKey2 = "exitKey2";
    public const string KeyExitKey3 = "exitKey3";
    public const string KeySebMode = "sebMode";
    public const string KeyBrowserMessagingSocket = "browserMessagingSocket";
    public const string KeyBrowserMessagingPingTime = "browserMessagingPingTime";
    public const string KeySebConfigPurpose = "sebConfigPurpose";
    public const string KeyAllowPreferencesWindow = "allowPreferencesWindow";
    public const string KeyCryptoIdentity = "cryptoIdentity";
    public const string KeyBrowserViewMode = "browserViewMode";
    public const string KeyMainBrowserWindowWidth = "mainBrowserWindowWidth";
    public const string KeyMainBrowserWindowHeight = "mainBrowserWindowHeight";
    public const string KeyMainBrowserWindowPositioning = "mainBrowserWindowPositioning";
    public const string KeyEnableBrowserWindowToolbar = "enableBrowserWindowToolbar";
    public const string KeyHideBrowserWindowToolbar = "hideBrowserWindowToolbar";
    public const string KeyShowMenuBar = "showMenuBar";
    public const string KeyShowTaskBar = "showTaskBar";
    public const string KeyTaskBarHeight = "taskBarHeight";
    public const string KeyTouchOptimized = "touchOptimized";
    public const string KeyEnableZoomText = "enableZoomText";
    public const string KeyEnableZoomPage = "enableZoomPage";
    public const string KeyZoomMode = "zoomMode";
    public const string KeyAllowSpellCheck = "allowSpellCheck";
    public const string KeyShowTime = "showTime";
    public const string KeyShowInputLanguage = "showInputLanguage";
    public const string KeyAllowDictionaryLookup = "allowDictionaryLookup";
    public const string KeyEnableTouchExit = "enableTouchExit";
    public const string KeyOskBehavior = "oskBehavior";
    public const string KeyBrowserScreenKeyboard = "browserScreenKeyboard";
    public const string KeyNewBrowserWindowByLinkPolicy = "newBrowserWindowByLinkPolicy";
    public const string KeyNewBrowserWindowByScriptPolicy = "newBrowserWindowByScriptPolicy";
    public const string KeyNewBrowserWindowByLinkBlockForeign = "newBrowserWindowByLinkBlockForeign";
    public const string KeyNewBrowserWindowByScriptBlockForeign = "newBrowserWindowByScriptBlockForeign";
    public const string KeyNewBrowserWindowByLinkWidth = "newBrowserWindowByLinkWidth";
    public const string KeyNewBrowserWindowByLinkHeight = "newBrowserWindowByLinkHeight";
    public const string KeyNewBrowserWindowByLinkPositioning = "newBrowserWindowByLinkPositioning";
    public const string KeyEnablePlugIns = "enablePlugIns";
    public const string KeyEnableJava = "enableJava";
    public const string KeyEnableJavaScript = "enableJavaScript";
    public const string KeyBlockPopUpWindows = "blockPopUpWindows";
    public const string KeyAllowBrowsingBackForward = "allowBrowsingBackForward";
    public const string KeyRemoveBrowserProfile = "removeBrowserProfile";
    public const string KeyDisableLocalStorage = "removeLocalStorage";
    public const string KeyEnableSebBrowser = "enableSebBrowser";
    public const string KeyShowReloadButton = "showReloadButton";
    public const string KeyShowReloadWarning = "showReloadWarning";
    public const string KeyBrowserUserAgentDesktopMode = "browserUserAgentWinDesktopMode";
    public const string KeyBrowserUserAgentDesktopModeCustom = "browserUserAgentWinDesktopModeCustom";
    public const string KeyBrowserUserAgentTouchMode = "browserUserAgentWinTouchMode";
    public const string KeyBrowserUserAgentTouchModeCustom = "browserUserAgentWinTouchModeCustom";
    public const string KeyBrowserUserAgent = "browserUserAgent";
    public const string KeyBrowserUserAgentMac = "browserUserAgentMac";
    public const string KeyBrowserUserAgentMacCustom = "browserUserAgentMacCustom";
    public const string KeyAllowDownUploads = "allowDownUploads";
    public const string KeyDownloadDirectoryOSX = "downloadDirectoryOSX";
    public const string KeyDownloadDirectoryWin = "downloadDirectoryWin";
    public const string KeyOpenDownloads = "openDownloads";
    public const string KeyChooseFileToUploadPolicy = "chooseFileToUploadPolicy";
    public const string KeyDownloadPDFFiles = "downloadPDFFiles";
    public const string KeyAllowPDFPlugIn = "allowPDFPlugIn";
    public const string KeyDownloadAndOpenSebConfig = "downloadAndOpenSebConfig";
    public const string KeyExamKeySalt = "examKeySalt";
    public const string KeyBrowserExamKey = "browserExamKey";
    public const string KeyBrowserURLSalt = "browserURLSalt";
    public const string KeySendBrowserExamKey = "sendBrowserExamKey";
    public const string KeyQuitURL = "quitURL";
    public const string KeyRestartExamText = "restartExamText";
    public const string KeyRestartExamURL = "restartExamURL";
    public const string KeyRestartExamUseStartURL = "restartExamUseStartURL";
    public const string KeyRestartExamPasswordProtected = "restartExamPasswordProtected";
    public const string KeyMonitorProcesses = "monitorProcesses";
    public const string KeyPermittedProcesses = "permittedProcesses";
    public const string KeyAllowSwitchToApplications = "allowSwitchToApplications";
    public const string KeyAllowFlashFullscreen = "allowFlashFullscreen";
    public const string KeyProhibitedProcesses = "prohibitedProcesses";
    public const string KeyActive = "active";
    public const string KeyAutostart = "autostart";
    public const string KeyIconInTaskbar = "iconInTaskbar";
    public const string KeyRunInBackground = "runInBackground";
    public const string KeyAllowUser = "allowUserToChooseApp";
    public const string KeyCurrentUser = "currentUser";
    public const string KeyStrongKill = "strongKill";
    public const string KeyOS = "os";
    public const string KeyTitle = "title";
    public const string KeyDescription = "description";
    public const string KeyExecutable = "executable";
    public const string KeyPath = "path";
    public const string KeyIdentifier = "identifier";
    public const string KeyUser = "user";
    public const string KeyArguments = "arguments";
    public const string KeyArgument = "argument";
    public const string KeyWindowHandlingProcess = "windowHandlingProcess";
    public const string KeyEnableURLFilter = "enableURLFilter";
    public const string KeyEnableURLContentFilter = "enableURLContentFilter";
    public const string KeyURLFilterEnable = "URLFilterEnable";
    public const string KeyURLFilterEnableContentFilter = "URLFilterEnableContentFilter";
    public const string KeyURLFilterRules = "URLFilterRules";
    public const string KeyUrlFilterBlacklist = "blacklistURLFilter";
    public const string KeyUrlFilterWhitelist = "whitelistURLFilter";
    public const string KeyUrlFilterTrustedContent = "urlFilterTrustedContent";
    public const string KeyUrlFilterRulesAsRegex = "urlFilterRegex";
    public const string KeyEmbeddedCertificates = "embeddedCertificates";
    public const string KeyCertificateDataWin = "certificateDataWin";
    public const string KeyCertificateData = "certificateData";
    public const string KeyType = "type";
    public const string KeyName = "name";
    public const string KeyProxySettingsPolicy = "proxySettingsPolicy";
    public const string KeyProxies = "proxies";
    public const string KeyExceptionsList = "ExceptionsList";
    public const string KeyExcludeSimpleHostnames = "ExcludeSimpleHostnames";
    public const string KeyFTPPassive = "FTPPassive";
    public const string KeyAutoDiscoveryEnabled = "AutoDiscoveryEnabled";
    public const string KeyAutoConfigurationEnabled = "AutoConfigurationEnabled";
    public const string KeyAutoConfigurationJavaScript = "AutoConfigurationJavaScript";
    public const string KeyAutoConfigurationURL = "AutoConfigurationURL";
    public const string KeyAutoDiscovery = "";
    public const string KeyAutoConfiguration = "";
    public const string KeyHTTP = "HTTP";
    public const string KeyHTTPS = "HTTPS";
    public const string KeyFTP = "FTP";
    public const string KeySOCKS = "SOCKS";
    public const string KeyRTSP = "RTSP";
    public const string KeyEnable = "Enable";
    public const string KeyPort = "Port";
    public const string KeyHost = "Proxy";
    public const string KeyRequires = "RequiresPassword";
    public const string KeyUsername = "Username";
    public const string KeyPassword = "Password";
    public const string KeyHTTPEnable = "HTTPEnable";
    public const string KeyHTTPPort = "HTTPPort";
    public const string KeyHTTPHost = "HTTPProxy";
    public const string KeyHTTPRequires = "HTTPRequiresPassword";
    public const string KeyHTTPUsername = "HTTPUsername";
    public const string KeyHTTPPassword = "HTTPPassword";
    public const string KeyHTTPSEnable = "HTTPSEnable";
    public const string KeyHTTPSPort = "HTTPSPort";
    public const string KeyHTTPSHost = "HTTPSProxy";
    public const string KeyHTTPSRequires = "HTTPSRequiresPassword";
    public const string KeyHTTPSUsername = "HTTPSUsername";
    public const string KeyHTTPSPassword = "HTTPSPassword";
    public const string KeyFTPEnable = "FTPEnable";
    public const string KeyFTPPort = "FTPPort";
    public const string KeyFTPHost = "FTPProxy";
    public const string KeyFTPRequires = "FTPRequiresPassword";
    public const string KeyFTPUsername = "FTPUsername";
    public const string KeyFTPPassword = "FTPPassword";
    public const string KeySOCKSEnable = "SOCKSEnable";
    public const string KeySOCKSPort = "SOCKSPort";
    public const string KeySOCKSHost = "SOCKSProxy";
    public const string KeySOCKSRequires = "SOCKSRequiresPassword";
    public const string KeySOCKSUsername = "SOCKSUsername";
    public const string KeySOCKSPassword = "SOCKSPassword";
    public const string KeyRTSPEnable = "RTSPEnable";
    public const string KeyRTSPPort = "RTSPPort";
    public const string KeyRTSPHost = "RTSPProxy";
    public const string KeyRTSPRequires = "RTSPRequiresPassword";
    public const string KeyRTSPUsername = "RTSPUsername";
    public const string KeyRTSPPassword = "RTSPPassword";
    public const string KeySebServicePolicy = "sebServicePolicy";
    public const string KeyAllowVirtualMachine = "allowVirtualMachine";
    public const string KeyCreateNewDesktop = "createNewDesktop";
    public const string KeyKillExplorerShell = "killExplorerShell";
    public const string KeyAllowUserSwitching = "allowUserSwitching";
    public const string KeyEnableAppSwitcherCheck = "enableAppSwitcherCheck";
    public const string KeyForceAppFolderInstall = "forceAppFolderInstall";
    public const string KeyEnableLogging = "enableLogging";
    public const string KeyLogDirectoryOSX = "logDirectoryOSX";
    public const string KeyLogDirectoryWin = "logDirectoryWin";
    public const string KeyAllowWLAN = "allowWlan";
    public const string KeyInsideSebEnableSwitchUser = "insideSebEnableSwitchUser";
    public const string KeyInsideSebEnableLockThisComputer = "insideSebEnableLockThisComputer";
    public const string KeyInsideSebEnableChangeAPassword = "insideSebEnableChangeAPassword";
    public const string KeyInsideSebEnableStartTaskManager = "insideSebEnableStartTaskManager";
    public const string KeyInsideSebEnableLogOff = "insideSebEnableLogOff";
    public const string KeyInsideSebEnableShutDown = "insideSebEnableShutDown";
    public const string KeyInsideSebEnableEaseOfAccess = "insideSebEnableEaseOfAccess";
    public const string KeyInsideSebEnableVmWareClientShade = "insideSebEnableVmWareClientShade";
    public const string KeyInsideSebEnableNetworkConnectionSelector = "insideSebEnableEnableNetworkConnectionSelector";
    public const string KeyHookKeys = "hookKeys";
    public const string KeyEnableEsc = "enableEsc";
    public const string KeyEnablePrintScreen = "enablePrintScreen";
    public const string KeyEnableCtrlEsc = "enableCtrlEsc";
    public const string KeyEnableAltEsc = "enableAltEsc";
    public const string KeyEnableAltTab = "enableAltTab";
    public const string KeyEnableAltF4 = "enableAltF4";
    public const string KeyEnableStartMenu = "enableStartMenu";
    public const string KeyEnableRightMouse = "enableRightMouse";
    public const string KeyEnableAltMouseWheel = "enableAltMouseWheel";
    public const string KeyEnableF1 = "enableF1";
    public const string KeyEnableF2 = "enableF2";
    public const string KeyEnableF3 = "enableF3";
    public const string KeyEnableF4 = "enableF4";
    public const string KeyEnableF5 = "enableF5";
    public const string KeyEnableF6 = "enableF6";
    public const string KeyEnableF7 = "enableF7";
    public const string KeyEnableF8 = "enableF8";
    public const string KeyEnableF9 = "enableF9";
    public const string KeyEnableF10 = "enableF10";
    public const string KeyEnableF11 = "enableF11";
    public const string KeyEnableF12 = "enableF12";
    public static int permittedProcessIndex;
    public static int permittedArgumentIndex;
    public static int prohibitedProcessIndex;
    public static int urlFilterRuleIndex;
    public static int urlFilterActionIndex;
    public static int embeddedCertificateIndex;
    public static int proxyProtocolIndex;
    public static int bypassedProxyIndex;

    public static void CreateDefaultAndCurrentSettingsFromScratch()
    {
      SEBSettings.settingsDefault = new Dictionary<string, object>();
      SEBSettings.settingsCurrent = new Dictionary<string, object>();
      SEBSettings.permittedProcessList = new List<object>();
      SEBSettings.permittedProcessData = new Dictionary<string, object>();
      SEBSettings.permittedProcessDataDefault = new Dictionary<string, object>();
      SEBSettings.permittedProcessDataXulRunner = new Dictionary<string, object>();
      SEBSettings.permittedArgumentList = new List<object>();
      SEBSettings.permittedArgumentData = new Dictionary<string, object>();
      SEBSettings.permittedArgumentDataDefault = new Dictionary<string, object>();
      SEBSettings.permittedArgumentDataXulRunner1 = new Dictionary<string, object>();
      SEBSettings.permittedArgumentDataXulRunner2 = new Dictionary<string, object>();
      SEBSettings.permittedArgumentListXulRunner = new List<object>();
      SEBSettings.prohibitedProcessList = new List<object>();
      SEBSettings.prohibitedProcessData = new Dictionary<string, object>();
      SEBSettings.prohibitedProcessDataDefault = new Dictionary<string, object>();
      SEBSettings.urlFilterRuleList = new List<object>();
      SEBSettings.urlFilterRuleData = new Dictionary<string, object>();
      SEBSettings.urlFilterRuleDataDefault = new Dictionary<string, object>();
      SEBSettings.urlFilterRuleDataStorage = new Dictionary<string, object>();
      SEBSettings.urlFilterActionList = new List<object>();
      SEBSettings.urlFilterActionListDefault = new List<object>();
      SEBSettings.urlFilterActionListStorage = new List<object>();
      SEBSettings.urlFilterActionData = new Dictionary<string, object>();
      SEBSettings.urlFilterActionDataDefault = new Dictionary<string, object>();
      SEBSettings.urlFilterActionDataStorage = new Dictionary<string, object>();
      SEBSettings.embeddedCertificateList = new List<object>();
      SEBSettings.embeddedCertificateData = new Dictionary<string, object>();
      SEBSettings.embeddedCertificateDataDefault = new Dictionary<string, object>();
      SEBSettings.proxiesData = new Dictionary<string, object>();
      SEBSettings.proxiesDataDefault = new Dictionary<string, object>();
      SEBSettings.bypassedProxyList = new List<object>();
      SEBSettings.bypassedProxyData = "";
      SEBSettings.bypassedProxyDataDefault = "";
      for (int index = 1; index <= 6; ++index)
      {
        SEBSettings.intArrayDefault[index] = 0;
        SEBSettings.intArrayCurrent[index] = 0;
        SEBSettings.strArrayDefault[index] = "";
        SEBSettings.strArrayCurrent[index] = "";
      }
      SEBSettings.settingsDefault.Clear();
      SEBSettings.settingsDefault.Add("originatorVersion", (object) "SEB_Win_2.1.1");
      SEBSettings.settingsDefault.Add("startURL", (object) "http://www.safeexambrowser.org/start");
      SEBSettings.settingsDefault.Add("sebServerURL", (object) "");
      SEBSettings.settingsDefault.Add("hashedAdminPassword", (object) "");
      SEBSettings.settingsDefault.Add("allowQuit", (object) true);
      SEBSettings.settingsDefault.Add("ignoreExitKeys", (object) true);
      SEBSettings.settingsDefault.Add("hashedQuitPassword", (object) "");
      SEBSettings.settingsDefault.Add("exitKey1", (object) 2);
      SEBSettings.settingsDefault.Add("exitKey2", (object) 10);
      SEBSettings.settingsDefault.Add("exitKey3", (object) 5);
      SEBSettings.settingsDefault.Add("sebMode", (object) 0);
      SEBSettings.settingsDefault.Add("browserMessagingSocket", (object) SEBXULRunnerWebSocketServer.ServerAddress);
      SEBSettings.settingsDefault.Add("browserMessagingPingTime", (object) 120000);
      SEBSettings.settingsDefault.Add("sebConfigPurpose", (object) 1);
      SEBSettings.settingsDefault.Add("allowPreferencesWindow", (object) true);
      SEBSettings.intArrayDefault[1] = 0;
      SEBSettings.strArrayDefault[1] = "";
      SEBSettings.settingsDefault.Add("browserViewMode", (object) 0);
      SEBSettings.settingsDefault.Add("mainBrowserWindowWidth", (object) "100%");
      SEBSettings.settingsDefault.Add("mainBrowserWindowHeight", (object) "100%");
      SEBSettings.settingsDefault.Add("mainBrowserWindowPositioning", (object) 1);
      SEBSettings.settingsDefault.Add("enableBrowserWindowToolbar", (object) false);
      SEBSettings.settingsDefault.Add("hideBrowserWindowToolbar", (object) false);
      SEBSettings.settingsDefault.Add("showMenuBar", (object) false);
      SEBSettings.settingsDefault.Add("showTaskBar", (object) true);
      SEBSettings.settingsDefault.Add("taskBarHeight", (object) 40);
      SEBSettings.settingsDefault.Add("touchOptimized", (object) false);
      SEBSettings.settingsDefault.Add("enableZoomText", (object) true);
      SEBSettings.settingsDefault.Add("enableZoomPage", (object) true);
      SEBSettings.settingsDefault.Add("zoomMode", (object) 0);
      SEBSettings.settingsDefault.Add("allowSpellCheck", (object) false);
      SEBSettings.settingsDefault.Add("allowDictionaryLookup", (object) false);
      SEBSettings.settingsDefault.Add("showTime", (object) true);
      SEBSettings.settingsDefault.Add("showInputLanguage", (object) true);
      SEBSettings.settingsDefault.Add("enableTouchExit", (object) false);
      SEBSettings.settingsDefault.Add("oskBehavior", (object) 2);
      SEBSettings.settingsDefault.Add("browserScreenKeyboard", (object) false);
      SEBSettings.intArrayDefault[2] = 2;
      SEBSettings.intArrayDefault[3] = 2;
      SEBSettings.strArrayDefault[2] = "100%";
      SEBSettings.strArrayDefault[3] = "100%";
      SEBSettings.settingsDefault.Add("newBrowserWindowByLinkPolicy", (object) 2);
      SEBSettings.settingsDefault.Add("newBrowserWindowByScriptPolicy", (object) 2);
      SEBSettings.settingsDefault.Add("newBrowserWindowByLinkBlockForeign", (object) false);
      SEBSettings.settingsDefault.Add("newBrowserWindowByScriptBlockForeign", (object) false);
      SEBSettings.settingsDefault.Add("newBrowserWindowByLinkWidth", (object) "1000");
      SEBSettings.settingsDefault.Add("newBrowserWindowByLinkHeight", (object) "100%");
      SEBSettings.settingsDefault.Add("newBrowserWindowByLinkPositioning", (object) 2);
      SEBSettings.settingsDefault.Add("enablePlugIns", (object) true);
      SEBSettings.settingsDefault.Add("enableJava", (object) false);
      SEBSettings.settingsDefault.Add("enableJavaScript", (object) true);
      SEBSettings.settingsDefault.Add("blockPopUpWindows", (object) false);
      SEBSettings.settingsDefault.Add("allowBrowsingBackForward", (object) false);
      SEBSettings.settingsDefault.Add("removeBrowserProfile", (object) true);
      SEBSettings.settingsDefault.Add("removeLocalStorage", (object) true);
      SEBSettings.settingsDefault.Add("enableSebBrowser", (object) true);
      SEBSettings.settingsDefault.Add("showReloadButton", (object) true);
      SEBSettings.settingsDefault.Add("showReloadWarning", (object) true);
      SEBSettings.settingsDefault.Add("browserUserAgentWinDesktopMode", (object) 0);
      SEBSettings.settingsDefault.Add("browserUserAgentWinDesktopModeCustom", (object) "");
      SEBSettings.settingsDefault.Add("browserUserAgentWinTouchMode", (object) 0);
      SEBSettings.settingsDefault.Add("browserUserAgentWinTouchModeCustom", (object) "");
      SEBSettings.settingsDefault.Add("browserUserAgent", (object) "");
      SEBSettings.settingsDefault.Add("browserUserAgentMac", (object) 0);
      SEBSettings.settingsDefault.Add("browserUserAgentMacCustom", (object) "");
      SEBSettings.intArrayDefault[4] = 4;
      SEBSettings.intArrayDefault[5] = 2;
      SEBSettings.strArrayDefault[4] = "1000";
      SEBSettings.strArrayDefault[5] = "100%";
      SEBSettings.settingsDefault.Add("allowDownUploads", (object) true);
      SEBSettings.settingsDefault.Add("downloadDirectoryOSX", (object) "~/Downloads");
      SEBSettings.settingsDefault.Add("downloadDirectoryWin", (object) "");
      SEBSettings.settingsDefault.Add("openDownloads", (object) false);
      SEBSettings.settingsDefault.Add("chooseFileToUploadPolicy", (object) 0);
      SEBSettings.settingsDefault.Add("downloadPDFFiles", (object) false);
      SEBSettings.settingsDefault.Add("allowPDFPlugIn", (object) false);
      SEBSettings.settingsDefault.Add("downloadAndOpenSebConfig", (object) true);
      SEBSettings.settingsDefault.Add("examKeySalt", (object) new byte[0]);
      SEBSettings.settingsDefault.Add("browserExamKey", (object) "");
      SEBSettings.settingsDefault.Add("browserURLSalt", (object) true);
      SEBSettings.settingsDefault.Add("sendBrowserExamKey", (object) false);
      SEBSettings.settingsDefault.Add("quitURL", (object) "");
      SEBSettings.settingsDefault.Add("restartExamURL", (object) "");
      SEBSettings.settingsDefault.Add("restartExamUseStartURL", (object) false);
      SEBSettings.settingsDefault.Add("restartExamText", (object) "");
      SEBSettings.settingsDefault.Add("restartExamPasswordProtected", (object) true);
      SEBSettings.settingsDefault.Add("monitorProcesses", (object) true);
      SEBSettings.settingsDefault.Add("allowSwitchToApplications", (object) false);
      SEBSettings.settingsDefault.Add("allowFlashFullscreen", (object) false);
      SEBSettings.settingsDefault.Add("permittedProcesses", (object) new List<object>());
      SEBSettings.settingsDefault.Add("prohibitedProcesses", (object) new List<object>());
      SEBSettings.permittedArgumentDataDefault.Clear();
      SEBSettings.permittedArgumentDataDefault.Add("active", (object) true);
      SEBSettings.permittedArgumentDataDefault.Add("argument", (object) "");
      SEBSettings.permittedArgumentListXulRunner.Clear();
      SEBSettings.permittedArgumentListXulRunner.Add((object) SEBSettings.permittedArgumentDataDefault);
      SEBSettings.permittedProcessDataXulRunner.Clear();
      SEBSettings.permittedProcessDataXulRunner.Add("active", (object) true);
      SEBSettings.permittedProcessDataXulRunner.Add("autostart", (object) true);
      SEBSettings.permittedProcessDataXulRunner.Add("iconInTaskbar", (object) true);
      SEBSettings.permittedProcessDataXulRunner.Add("runInBackground", (object) false);
      SEBSettings.permittedProcessDataXulRunner.Add("allowUserToChooseApp", (object) false);
      SEBSettings.permittedProcessDataXulRunner.Add("strongKill", (object) true);
      SEBSettings.permittedProcessDataXulRunner.Add("os", (object) 1);
      SEBSettings.permittedProcessDataXulRunner.Add("title", (object) "SEB");
      SEBSettings.permittedProcessDataXulRunner.Add("description", (object) "");
      SEBSettings.permittedProcessDataXulRunner.Add("executable", (object) "xulrunner.exe");
      SEBSettings.permittedProcessDataXulRunner.Add("path", (object) "../xulrunner/");
      SEBSettings.permittedProcessDataXulRunner.Add("identifier", (object) "XULRunner");
      SEBSettings.permittedProcessDataXulRunner.Add("windowHandlingProcess", (object) "");
      SEBSettings.permittedProcessDataXulRunner.Add("arguments", (object) new List<object>());
      SEBSettings.permittedProcessDataDefault.Clear();
      SEBSettings.permittedProcessDataDefault.Add("active", (object) true);
      SEBSettings.permittedProcessDataDefault.Add("autostart", (object) true);
      SEBSettings.permittedProcessDataDefault.Add("iconInTaskbar", (object) true);
      SEBSettings.permittedProcessDataDefault.Add("runInBackground", (object) false);
      SEBSettings.permittedProcessDataDefault.Add("allowUserToChooseApp", (object) false);
      SEBSettings.permittedProcessDataDefault.Add("strongKill", (object) false);
      SEBSettings.permittedProcessDataDefault.Add("os", (object) 1);
      SEBSettings.permittedProcessDataDefault.Add("title", (object) "");
      SEBSettings.permittedProcessDataDefault.Add("description", (object) "");
      SEBSettings.permittedProcessDataDefault.Add("executable", (object) "");
      SEBSettings.permittedProcessDataDefault.Add("path", (object) "");
      SEBSettings.permittedProcessDataDefault.Add("identifier", (object) "");
      SEBSettings.permittedProcessDataDefault.Add("windowHandlingProcess", (object) "");
      SEBSettings.permittedProcessDataDefault.Add("arguments", (object) new List<object>());
      SEBSettings.prohibitedProcessDataDefault.Clear();
      SEBSettings.prohibitedProcessDataDefault.Add("active", (object) true);
      SEBSettings.prohibitedProcessDataDefault.Add("currentUser", (object) true);
      SEBSettings.prohibitedProcessDataDefault.Add("strongKill", (object) false);
      SEBSettings.prohibitedProcessDataDefault.Add("os", (object) 1);
      SEBSettings.prohibitedProcessDataDefault.Add("executable", (object) "");
      SEBSettings.prohibitedProcessDataDefault.Add("description", (object) "");
      SEBSettings.prohibitedProcessDataDefault.Add("identifier", (object) "");
      SEBSettings.prohibitedProcessDataDefault.Add("windowHandlingProcess", (object) "");
      SEBSettings.prohibitedProcessDataDefault.Add("user", (object) "");
      SEBSettings.settingsDefault.Add("enableURLFilter", (object) false);
      SEBSettings.settingsDefault.Add("enableURLContentFilter", (object) false);
      SEBSettings.settingsDefault.Add("URLFilterRules", (object) new List<object>());
      SEBSettings.settingsDefault.Add("URLFilterEnable", (object) false);
      SEBSettings.settingsDefault.Add("URLFilterEnableContentFilter", (object) false);
      SEBSettings.settingsDefault.Add("blacklistURLFilter", (object) "");
      SEBSettings.settingsDefault.Add("whitelistURLFilter", (object) "");
      SEBSettings.settingsDefault.Add("urlFilterTrustedContent", (object) false);
      SEBSettings.settingsDefault.Add("urlFilterRegex", (object) false);
      SEBSettings.settingsDefault.Add("embeddedCertificates", (object) new List<object>());
      SEBSettings.embeddedCertificateDataDefault.Clear();
      SEBSettings.embeddedCertificateDataDefault.Add("certificateData", (object) new byte[0]);
      SEBSettings.embeddedCertificateDataDefault.Add("certificateDataWin", (object) "");
      SEBSettings.embeddedCertificateDataDefault.Add("type", (object) 0);
      SEBSettings.embeddedCertificateDataDefault.Add("name", (object) "");
      SEBSettings.proxiesDataDefault.Clear();
      SEBSettings.proxiesDataDefault.Add("ExceptionsList", (object) new List<object>());
      SEBSettings.proxiesDataDefault.Add("ExcludeSimpleHostnames", (object) false);
      SEBSettings.proxiesDataDefault.Add("AutoDiscoveryEnabled", (object) false);
      SEBSettings.proxiesDataDefault.Add("AutoConfigurationEnabled", (object) false);
      SEBSettings.proxiesDataDefault.Add("AutoConfigurationJavaScript", (object) "");
      SEBSettings.proxiesDataDefault.Add("AutoConfigurationURL", (object) "");
      SEBSettings.proxiesDataDefault.Add("FTPPassive", (object) true);
      SEBSettings.proxiesDataDefault.Add("HTTPEnable", (object) false);
      SEBSettings.proxiesDataDefault.Add("HTTPPort", (object) 80);
      SEBSettings.proxiesDataDefault.Add("HTTPProxy", (object) "");
      SEBSettings.proxiesDataDefault.Add("HTTPRequiresPassword", (object) false);
      SEBSettings.proxiesDataDefault.Add("HTTPUsername", (object) "");
      SEBSettings.proxiesDataDefault.Add("HTTPPassword", (object) "");
      SEBSettings.proxiesDataDefault.Add("HTTPSEnable", (object) false);
      SEBSettings.proxiesDataDefault.Add("HTTPSPort", (object) 443);
      SEBSettings.proxiesDataDefault.Add("HTTPSProxy", (object) "");
      SEBSettings.proxiesDataDefault.Add("HTTPSRequiresPassword", (object) false);
      SEBSettings.proxiesDataDefault.Add("HTTPSUsername", (object) "");
      SEBSettings.proxiesDataDefault.Add("HTTPSPassword", (object) "");
      SEBSettings.proxiesDataDefault.Add("FTPEnable", (object) false);
      SEBSettings.proxiesDataDefault.Add("FTPPort", (object) 21);
      SEBSettings.proxiesDataDefault.Add("FTPProxy", (object) "");
      SEBSettings.proxiesDataDefault.Add("FTPRequiresPassword", (object) false);
      SEBSettings.proxiesDataDefault.Add("FTPUsername", (object) "");
      SEBSettings.proxiesDataDefault.Add("FTPPassword", (object) "");
      SEBSettings.proxiesDataDefault.Add("SOCKSEnable", (object) false);
      SEBSettings.proxiesDataDefault.Add("SOCKSPort", (object) 1080);
      SEBSettings.proxiesDataDefault.Add("SOCKSProxy", (object) "");
      SEBSettings.proxiesDataDefault.Add("SOCKSRequiresPassword", (object) false);
      SEBSettings.proxiesDataDefault.Add("SOCKSUsername", (object) "");
      SEBSettings.proxiesDataDefault.Add("SOCKSPassword", (object) "");
      SEBSettings.proxiesDataDefault.Add("RTSPEnable", (object) false);
      SEBSettings.proxiesDataDefault.Add("RTSPPort", (object) 554);
      SEBSettings.proxiesDataDefault.Add("RTSPProxy", (object) "");
      SEBSettings.proxiesDataDefault.Add("RTSPRequiresPassword", (object) false);
      SEBSettings.proxiesDataDefault.Add("RTSPUsername", (object) "");
      SEBSettings.proxiesDataDefault.Add("RTSPPassword", (object) "");
      SEBSettings.bypassedProxyDataDefault = "";
      SEBSettings.settingsDefault.Add("proxySettingsPolicy", (object) 0);
      SEBSettings.settingsDefault.Add("proxies", (object) SEBSettings.proxiesDataDefault);
      SEBSettings.settingsDefault.Add("sebServicePolicy", (object) 1);
      SEBSettings.settingsDefault.Add("allowVirtualMachine", (object) false);
      SEBSettings.settingsDefault.Add("createNewDesktop", (object) true);
      SEBSettings.settingsDefault.Add("killExplorerShell", (object) false);
      SEBSettings.settingsDefault.Add("allowUserSwitching", (object) true);
      SEBSettings.settingsDefault.Add("enableAppSwitcherCheck", (object) true);
      SEBSettings.settingsDefault.Add("forceAppFolderInstall", (object) true);
      SEBSettings.settingsDefault.Add("enableLogging", (object) true);
      SEBSettings.settingsDefault.Add("logDirectoryOSX", (object) "~/Documents");
      SEBSettings.settingsDefault.Add("logDirectoryWin", (object) "");
      SEBSettings.settingsDefault.Add("allowWlan", (object) false);
      SEBSettings.settingsDefault.Add("insideSebEnableSwitchUser", (object) false);
      SEBSettings.settingsDefault.Add("insideSebEnableLockThisComputer", (object) false);
      SEBSettings.settingsDefault.Add("insideSebEnableChangeAPassword", (object) false);
      SEBSettings.settingsDefault.Add("insideSebEnableStartTaskManager", (object) false);
      SEBSettings.settingsDefault.Add("insideSebEnableLogOff", (object) false);
      SEBSettings.settingsDefault.Add("insideSebEnableShutDown", (object) false);
      SEBSettings.settingsDefault.Add("insideSebEnableEaseOfAccess", (object) false);
      SEBSettings.settingsDefault.Add("insideSebEnableVmWareClientShade", (object) false);
      SEBSettings.settingsDefault.Add("insideSebEnableEnableNetworkConnectionSelector", (object) false);
      SEBSettings.settingsDefault.Add("hookKeys", (object) true);
      SEBSettings.settingsDefault.Add("enableEsc", (object) false);
      SEBSettings.settingsDefault.Add("enableCtrlEsc", (object) false);
      SEBSettings.settingsDefault.Add("enableAltEsc", (object) false);
      SEBSettings.settingsDefault.Add("enableAltTab", (object) true);
      SEBSettings.settingsDefault.Add("enableAltF4", (object) false);
      SEBSettings.settingsDefault.Add("enableStartMenu", (object) false);
      SEBSettings.settingsDefault.Add("enableRightMouse", (object) false);
      SEBSettings.settingsDefault.Add("enablePrintScreen", (object) false);
      SEBSettings.settingsDefault.Add("enableAltMouseWheel", (object) false);
      SEBSettings.settingsDefault.Add("enableF1", (object) false);
      SEBSettings.settingsDefault.Add("enableF2", (object) false);
      SEBSettings.settingsDefault.Add("enableF3", (object) false);
      SEBSettings.settingsDefault.Add("enableF4", (object) false);
      SEBSettings.settingsDefault.Add("enableF5", (object) true);
      SEBSettings.settingsDefault.Add("enableF6", (object) false);
      SEBSettings.settingsDefault.Add("enableF7", (object) false);
      SEBSettings.settingsDefault.Add("enableF8", (object) false);
      SEBSettings.settingsDefault.Add("enableF9", (object) false);
      SEBSettings.settingsDefault.Add("enableF10", (object) false);
      SEBSettings.settingsDefault.Add("enableF11", (object) false);
      SEBSettings.settingsDefault.Add("enableF12", (object) false);
      SEBSettings.permittedProcessIndex = -1;
      SEBSettings.permittedProcessList.Clear();
      SEBSettings.permittedProcessData.Clear();
      SEBSettings.permittedArgumentIndex = -1;
      SEBSettings.permittedArgumentList.Clear();
      SEBSettings.permittedArgumentData.Clear();
      SEBSettings.prohibitedProcessIndex = -1;
      SEBSettings.prohibitedProcessList.Clear();
      SEBSettings.prohibitedProcessData.Clear();
      SEBSettings.urlFilterRuleIndex = -1;
      SEBSettings.urlFilterRuleList.Clear();
      SEBSettings.urlFilterRuleData.Clear();
      SEBSettings.urlFilterActionIndex = -1;
      SEBSettings.urlFilterActionList.Clear();
      SEBSettings.urlFilterActionData.Clear();
      SEBSettings.embeddedCertificateIndex = -1;
      SEBSettings.embeddedCertificateList.Clear();
      SEBSettings.embeddedCertificateData.Clear();
      SEBSettings.proxyProtocolIndex = -1;
      SEBSettings.proxiesData.Clear();
      SEBSettings.bypassedProxyIndex = -1;
      SEBSettings.bypassedProxyList.Clear();
      SEBSettings.bypassedProxyData = "";
    }

    public static void RestoreDefaultAndCurrentSettings()
    {
      SEBSettings.CreateDefaultAndCurrentSettingsFromScratch();
      SEBSettings.settingsCurrent.Clear();
      SEBSettings.FillSettingsDictionary();
      SEBSettings.FillSettingsArrays();
    }

    public static void FillSettingsArrays()
    {
      for (int index = 1; index <= 6; ++index)
      {
        SEBSettings.intArrayCurrent[index] = SEBSettings.intArrayDefault[index];
        SEBSettings.strArrayCurrent[index] = SEBSettings.strArrayDefault[index];
      }
    }

    public static void FillSettingsDictionary()
    {
      foreach (KeyValuePair<string, object> keyValuePair in SEBSettings.settingsDefault)
      {
        if (!SEBSettings.settingsCurrent.ContainsKey(keyValuePair.Key))
        {
          SEBSettings.settingsCurrent.Add(keyValuePair.Key, keyValuePair.Value);
        }
        else
        {
          object obj1 = SEBSettings.settingsCurrent[keyValuePair.Key];
          object obj2 = keyValuePair.Value;
          if (!obj1.GetType().Equals(obj2.GetType()))
            SEBSettings.settingsCurrent[keyValuePair.Key] = obj2;
        }
      }
      SEBSettings.permittedProcessList = (List<object>) SEBSettings.settingsCurrent["permittedProcesses"];
      for (int index1 = 0; index1 < SEBSettings.permittedProcessList.Count; ++index1)
      {
        SEBSettings.permittedProcessData = (Dictionary<string, object>) SEBSettings.permittedProcessList[index1];
        foreach (KeyValuePair<string, object> keyValuePair in SEBSettings.permittedProcessDataDefault)
        {
          if (!SEBSettings.permittedProcessData.ContainsKey(keyValuePair.Key))
            SEBSettings.permittedProcessData.Add(keyValuePair.Key, keyValuePair.Value);
        }
        SEBSettings.permittedArgumentList = (List<object>) SEBSettings.permittedProcessData["arguments"];
        for (int index2 = 0; index2 < SEBSettings.permittedArgumentList.Count; ++index2)
        {
          SEBSettings.permittedArgumentData = (Dictionary<string, object>) SEBSettings.permittedArgumentList[index2];
          foreach (KeyValuePair<string, object> keyValuePair in SEBSettings.permittedArgumentDataDefault)
          {
            if (!SEBSettings.permittedArgumentData.ContainsKey(keyValuePair.Key) && keyValuePair.Value != (object) "")
              SEBSettings.permittedArgumentData.Add(keyValuePair.Key, keyValuePair.Value);
          }
        }
      }
      SEBSettings.prohibitedProcessList = (List<object>) SEBSettings.settingsCurrent["prohibitedProcesses"];
      for (int index = 0; index < SEBSettings.prohibitedProcessList.Count; ++index)
      {
        SEBSettings.prohibitedProcessData = (Dictionary<string, object>) SEBSettings.prohibitedProcessList[index];
        foreach (KeyValuePair<string, object> keyValuePair in SEBSettings.prohibitedProcessDataDefault)
        {
          if (!SEBSettings.prohibitedProcessData.ContainsKey(keyValuePair.Key))
            SEBSettings.prohibitedProcessData.Add(keyValuePair.Key, keyValuePair.Value);
        }
      }
      SEBSettings.embeddedCertificateList = (List<object>) SEBSettings.settingsCurrent["embeddedCertificates"];
      for (int index = 0; index < SEBSettings.embeddedCertificateList.Count; ++index)
      {
        SEBSettings.embeddedCertificateData = (Dictionary<string, object>) SEBSettings.embeddedCertificateList[index];
        foreach (KeyValuePair<string, object> keyValuePair in SEBSettings.embeddedCertificateDataDefault)
        {
          if (!SEBSettings.embeddedCertificateData.ContainsKey(keyValuePair.Key))
            SEBSettings.embeddedCertificateData.Add(keyValuePair.Key, keyValuePair.Value);
        }
      }
      SEBSettings.proxiesData = (Dictionary<string, object>) SEBSettings.settingsCurrent["proxies"];
      foreach (KeyValuePair<string, object> keyValuePair in SEBSettings.proxiesDataDefault)
      {
        if (!SEBSettings.proxiesData.ContainsKey(keyValuePair.Key))
          SEBSettings.proxiesData.Add(keyValuePair.Key, keyValuePair.Value);
      }
      SEBSettings.bypassedProxyList = (List<object>) SEBSettings.proxiesData["ExceptionsList"];
      for (int index = 0; index < SEBSettings.bypassedProxyList.Count; ++index)
      {
        if ((string) SEBSettings.bypassedProxyList[index] == "")
          SEBSettings.bypassedProxyList[index] = (object) SEBSettings.bypassedProxyDataDefault;
      }
    }

    public static Dictionary<string, object> CleanSettingsDictionary()
    {
      Dictionary<string, object> dictionary1 = new Dictionary<string, object>();
      foreach (KeyValuePair<string, object> keyValuePair in SEBSettings.settingsDefault)
      {
        if ((!(keyValuePair.Value is List<object>) || ((List<object>) keyValuePair.Value).Count != 0) && (!(keyValuePair.Value is Dictionary<string, object>) || ((Dictionary<string, object>) keyValuePair.Value).Count != 0))
          dictionary1.Add(keyValuePair.Key, keyValuePair.Value);
      }
      List<object> objectList1 = (List<object>) SEBSettings.valueForDictionaryKey(dictionary1, "permittedProcesses");
      if (objectList1 != null)
      {
        for (int index1 = 0; index1 < objectList1.Count; ++index1)
        {
          Dictionary<string, object> dictionary2 = (Dictionary<string, object>) objectList1[index1];
          if (dictionary2 != null)
          {
            foreach (KeyValuePair<string, object> keyValuePair in SEBSettings.permittedProcessDataDefault)
            {
              if (!dictionary2.ContainsKey(keyValuePair.Key) && (!(keyValuePair.Value is List<object>) || ((List<object>) keyValuePair.Value).Count != 0) && (!(keyValuePair.Value is Dictionary<string, object>) || ((Dictionary<string, object>) keyValuePair.Value).Count != 0))
                dictionary2.Add(keyValuePair.Key, keyValuePair.Value);
            }
            List<object> objectList2 = (List<object>) SEBSettings.valueForDictionaryKey(dictionary2, "arguments");
            if (objectList2 != null)
            {
              for (int index2 = 0; index2 < objectList2.Count; ++index2)
              {
                Dictionary<string, object> dictionary3 = (Dictionary<string, object>) objectList2[index2];
                foreach (KeyValuePair<string, object> keyValuePair in SEBSettings.permittedArgumentDataDefault)
                {
                  if (!dictionary3.ContainsKey(keyValuePair.Key) && keyValuePair.Value != (object) "")
                    dictionary3.Add(keyValuePair.Key, keyValuePair.Value);
                }
              }
            }
          }
        }
      }
      List<object> objectList3 = (List<object>) SEBSettings.valueForDictionaryKey(dictionary1, "prohibitedProcesses");
      if (objectList3 != null)
      {
        for (int index = 0; index < objectList3.Count; ++index)
        {
          Dictionary<string, object> dictionary2 = (Dictionary<string, object>) objectList3[index];
          foreach (KeyValuePair<string, object> keyValuePair in SEBSettings.prohibitedProcessDataDefault)
          {
            if ((!(keyValuePair.Value is List<object>) || ((List<object>) keyValuePair.Value).Count != 0) && (!(keyValuePair.Value is Dictionary<string, object>) || ((Dictionary<string, object>) keyValuePair.Value).Count != 0))
              dictionary2.Add(keyValuePair.Key, keyValuePair.Value);
          }
        }
      }
      List<object> objectList4 = (List<object>) SEBSettings.valueForDictionaryKey(dictionary1, "embeddedCertificates");
      if (objectList4 != null)
      {
        for (int index = 0; index < objectList4.Count; ++index)
        {
          Dictionary<string, object> dictionary2 = (Dictionary<string, object>) objectList4[index];
          foreach (KeyValuePair<string, object> keyValuePair in SEBSettings.embeddedCertificateDataDefault)
          {
            if ((!(keyValuePair.Value is List<object>) || ((List<object>) keyValuePair.Value).Count != 0) && (!(keyValuePair.Value is Dictionary<string, object>) || ((Dictionary<string, object>) keyValuePair.Value).Count != 0))
              dictionary2.Add(keyValuePair.Key, keyValuePair.Value);
          }
        }
      }
      Dictionary<string, object> dictionary4 = (Dictionary<string, object>) SEBSettings.valueForDictionaryKey(dictionary1, "proxies");
      if (dictionary4 != null)
      {
        foreach (KeyValuePair<string, object> keyValuePair in SEBSettings.proxiesDataDefault)
        {
          if (!dictionary4.ContainsKey(keyValuePair.Key) && (!(keyValuePair.Value is List<object>) || ((List<object>) keyValuePair.Value).Count != 0) && (!(keyValuePair.Value is Dictionary<string, object>) || ((Dictionary<string, object>) keyValuePair.Value).Count != 0))
            dictionary4.Add(keyValuePair.Key, keyValuePair.Value);
        }
        List<object> objectList2 = (List<object>) SEBSettings.valueForDictionaryKey(dictionary4, "ExceptionsList");
        if (objectList2 != null && objectList2.Count != 0)
        {
          for (int index = 0; index < objectList2.Count; ++index)
          {
            if ((string) objectList2[index] == "")
              objectList2[index] = (object) SEBSettings.bypassedProxyDataDefault;
          }
        }
      }
      return dictionary1;
    }

    public static object valueForDictionaryKey(Dictionary<string, object> dictionary, string key)
    {
      if (dictionary.ContainsKey(key))
        return dictionary[key];
      return (object) null;
    }

    public static Dictionary<TKey, TValue> CloneDictionaryCloningValues<TKey, TValue>(Dictionary<TKey, TValue> original) where TValue : ICloneable
    {
      Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>(original.Count, original.Comparer);
      foreach (KeyValuePair<TKey, TValue> keyValuePair in original)
        dictionary.Add(keyValuePair.Key, (TValue) keyValuePair.Value.Clone());
      return dictionary;
    }

    public static void PermitXulRunnerProcess()
    {
      SEBSettings.permittedProcessList = (List<object>) SEBSettings.settingsCurrent["permittedProcesses"];
      int num = -1;
      for (int index = 0; index < SEBSettings.permittedProcessList.Count; ++index)
      {
        SEBSettings.permittedProcessData = (Dictionary<string, object>) SEBSettings.permittedProcessList[index];
        if (SEBSettings.permittedProcessData["executable"].Equals((object) "xulrunner.exe"))
          num = index;
      }
      if (num != -1)
        return;
      SEBSettings.permittedProcessList.Add((object) SEBSettings.permittedProcessDataXulRunner);
    }

    public static void PrintSettingsRecursively(object objectSource, StreamWriter fileWriter, string indenting)
    {
      string str1 = objectSource.GetType().ToString();
      if (str1.Contains("Dictionary"))
      {
        Dictionary<string, object> source = (Dictionary<string, object>) objectSource;
        for (int index = 0; index < source.Count; ++index)
        {
          KeyValuePair<string, object> keyValuePair = source.ElementAt<KeyValuePair<string, object>>(index);
          string key = keyValuePair.Key;
          object obj = keyValuePair.Value;
          string str2 = keyValuePair.Value.GetType().ToString();
          fileWriter.WriteLine(indenting + key + "=" + obj);
          if (str2.Contains("Dictionary") || str2.Contains("List"))
            SEBSettings.PrintSettingsRecursively(source[key], fileWriter, indenting + "   ");
        }
      }
      if (!str1.Contains("List"))
        return;
      List<object> objectList = (List<object>) objectSource;
      for (int index = 0; index < objectList.Count; ++index)
      {
        object obj = objectList[index];
        string str2 = obj.GetType().ToString();
        fileWriter.WriteLine(indenting + obj);
        if (str2.Contains("Dictionary") || str2.Contains("List"))
          SEBSettings.PrintSettingsRecursively(objectList[index], fileWriter, indenting + "   ");
      }
    }

    public static void LoggSettingsDictionary(ref Dictionary<string, object> sebSettings, string fileName)
    {
      if (File.Exists(fileName))
        File.Delete(fileName);
      FileStream fileStream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write);
      StreamWriter fileWriter = new StreamWriter((Stream) fileStream);
      fileWriter.WriteLine("");
      fileWriter.WriteLine("number of (key, value) pairs = " + (object) sebSettings.Count);
      fileWriter.WriteLine("");
      SEBSettings.PrintSettingsRecursively((object) sebSettings, fileWriter, "");
      fileWriter.Close();
      fileStream.Close();
    }

    public static bool StoreDecryptedSebClientSettings(byte[] sebSettings)
    {
      Dictionary<string, object> settingsDict = (Dictionary<string, object>) null;
      if (sebSettings != null)
      {
        string sebFilePassword = (string) null;
        bool passwordIsHash = false;
        X509Certificate2 sebFileCertificateRef = (X509Certificate2) null;
        try
        {
          settingsDict = SEBConfigFileManager.DecryptSEBSettings(sebSettings, false, ref sebFilePassword, ref passwordIsHash, ref sebFileCertificateRef);
          if (settingsDict == null)
          {
            Logger.AddError("The .seb file could not be decrypted. ", (object) null, (Exception) null, "");
            return false;
          }
        }
        catch (Exception ex)
        {
          Logger.AddError("The .seb file could not be decrypted. ", (object) null, ex, ex.Message);
          return false;
        }
      }
      SEBSettings.StoreSebClientSettings(settingsDict);
      return true;
    }

    public static void StoreSebClientSettings(Dictionary<string, object> settingsDict)
    {
      SEBSettings.CreateDefaultAndCurrentSettingsFromScratch();
      SEBSettings.settingsCurrent.Clear();
      if (settingsDict != null)
        SEBSettings.settingsCurrent = settingsDict;
      SEBSettings.FillSettingsDictionary();
      SEBSettings.FillSettingsArrays();
      SEBSettings.PermitXulRunnerProcess();
    }

    public static bool ReadSebConfigurationFile(string fileName, bool forEditing, ref string filePassword, ref bool passwordIsHash, ref X509Certificate2 fileCertificateRef)
    {
      Dictionary<string, object> dictionary1 = new Dictionary<string, object>();
      Dictionary<string, object> dictionary2;
      try
      {
        dictionary2 = SEBConfigFileManager.DecryptSEBSettings(File.ReadAllBytes(fileName), forEditing, ref filePassword, ref passwordIsHash, ref fileCertificateRef);
        if (dictionary2 == null)
          return false;
      }
      catch (Exception ex)
      {
        Logger.AddError("The .seb file could not be read: ", (object) null, ex, ex.Message);
        return false;
      }
      SEBSettings.CreateDefaultAndCurrentSettingsFromScratch();
      SEBSettings.settingsCurrent.Clear();
      SEBSettings.settingsCurrent = dictionary2;
      SEBSettings.FillSettingsDictionary();
      SEBSettings.FillSettingsArrays();
      SEBSettings.PermitXulRunnerProcess();
      return true;
    }

    public static bool WriteSebConfigurationFile(string fileName, string filePassword, bool passwordIsHash, X509Certificate2 fileCertificateRef, SEBSettings.sebConfigPurposes configPurpose, bool forEditing = false)
    {
      try
      {
        byte[] bytes = SEBConfigFileManager.EncryptSEBSettingsWithCredentials(filePassword, passwordIsHash, fileCertificateRef, configPurpose, forEditing);
        File.WriteAllBytes(fileName, bytes);
      }
      catch (Exception ex)
      {
        Logger.AddError("The configuration file could not be written: ", (object) null, ex, ex.Message);
        return false;
      }
      return true;
    }

    public enum sebConfigPurposes
    {
      sebConfigPurposeStartingExam,
      sebConfigPurposeConfiguringClient,
    }

    public enum operatingSystems
    {
      operatingSystemOSX,
      operatingSystemWin,
    }
  }
}
