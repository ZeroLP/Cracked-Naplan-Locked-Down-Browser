
using SebWindowsClient.ConfigurationUtils;
using SebWindowsClient.DesktopUtils;
using SebWindowsClient.DiagnosticsUtils;
using SebWindowsClient.ProcessUtils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace SebWindowsClient
{
  internal static class SebWindowsClientMain
  {
    private static IntPtr vistaStartMenuWnd = IntPtr.Zero;
    private static volatile bool _loadingSebFile = false;
    public static SingleInstanceController singleInstanceController;
    private const int SW_HIDE = 0;
    private const int SW_SHOW = 5;
    private const int SW_RESTORE = 9;
    private const string VistaStartMenuCaption = "Start";
    public static bool sessionCreateNewDesktop;
    public static SEBSplashScreen splash;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll")]
    private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

    [DllImport("user32.dll")]
    private static extern IntPtr GetDesktopWindow();

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern bool EnumThreadWindows(int threadId, SebWindowsClientMain.EnumThreadProc pfnEnum, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowTitle);

    [DllImport("user32.dll")]
    private static extern IntPtr FindWindowEx(IntPtr parentHwnd, IntPtr childAfterHwnd, IntPtr className, string windowText);

    [DllImport("User32.dll")]
    private static extern bool IsIconic(IntPtr handle);

    [DllImport("user32.dll")]
    private static extern int ShowWindow(IntPtr hwnd, int nCmdShow);

    [DllImport("User32.dll")]
    public static extern IntPtr SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hwnd, out int lpdwProcessId);

    [DllImport("kernel32.dll")]
    public static extern IntPtr LoadLibrary(string dllToLoad);

    [DllImport("kernel32.dll")]
    public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

    [DllImport("kernel32.dll")]
    public static extern bool FreeLibrary(IntPtr hModule);

    public static bool clientSettingsSet { get; set; }

    [STAThread]
    private static void Main()
    {
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      string[] commandLineArgs = Environment.GetCommandLineArgs();
      Logger.AddInformation("---------- INITIALIZING SEB - STARTING SESSION -------------", (object) null, (Exception) null, (string) null);
      Logger.AddInformation(" Arguments: " + string.Join(", ", commandLineArgs), (object) null, (Exception) null, (string) null);
      try
      {
        if (!SebWindowsClientMain.InitSebSettings())
          return;
      }
      catch (Exception ex)
      {
        Logger.AddError("Unable to InitSebSettings", (object) null, ex, (string) null);
        return;
      }
      /*try
      {
        SebWindowsClientMain.CheckDistributionIntegrity();
      }
      catch (Exception ex)
      {
        int num = (int) SEBMessageBox.Show(SEBUIStrings.integrityCheckError, SEBUIStrings.integrityCheckErrorReason, MessageBoxIcon.Hand, MessageBoxButtons.OK, false);
        Logger.AddError(SEBUIStrings.integrityCheckError, (object) null, ex, (string) null);
        return;
      }
      */
      SebWindowsClientMain.singleInstanceController = new SingleInstanceController();
      try
      {
        SEBClientInfo.SebWindowsClientForm = new SebWindowsClientForm();
        SebWindowsClientMain.singleInstanceController.Run(commandLineArgs);
      }
      catch (Exception ex)
      {
        Logger.AddError(ex.Message, (object) null, ex, (string) null);
      }
    }

    public static void StartSplash()
    {
      if ((bool) SEBClientInfo.getSebSetting("createNewDesktop")["createNewDesktop"])
        SEBDesktopController.SetCurrent(SEBClientInfo.SEBNewlDesktop);
      SebWindowsClientMain.splash = new SEBSplashScreen();
      Application.Run((Form) SebWindowsClientMain.splash);
    }

    public static void CloseSplash()
    {
      if (SebWindowsClientMain.splash == null)
        return;
      try
      {
        SebWindowsClientMain.splash.Invoke((Delegate) new EventHandler(SebWindowsClientMain.splash.KillMe));
        SebWindowsClientMain.splash.Dispose();
        SebWindowsClientMain.splash = (SEBSplashScreen) null;
      }
      catch (Exception ex)
      {
      }
    }

    public static void LoadingSebFile(bool loading)
    {
      SebWindowsClientMain._loadingSebFile = loading;
    }

    public static bool isLoadingSebFile()
    {
      return SebWindowsClientMain._loadingSebFile;
    }

    private static bool IsInsideVM()
    {
      using (ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher("Select * from Win32_ComputerSystem"))
      {
        using (ManagementObjectCollection objectCollection = managementObjectSearcher.Get())
        {
          foreach (ManagementBaseObject managementBaseObject in objectCollection)
          {
            Logger.AddInformation("Win32_ComputerSystem Manufacturer: " + managementBaseObject["Manufacturer"].ToString() + ", Model: " + managementBaseObject["Model"].ToString(), (object) null, (Exception) null, (string) null);
            string lower1 = managementBaseObject["Manufacturer"].ToString().ToLower();
            string lower2 = managementBaseObject["Model"].ToString().ToLower();
            if (lower1 == "microsoft corporation" && !lower2.Contains("surface") || (lower1.Contains("vmware") || lower1.Contains("parallels software")) || (lower1.Contains("xen") || lower2.Contains("xen") || lower2.Contains("virtualbox")))
              return true;
          }
        }
      }
      return false;
    }

    public static bool InitSebSettings()
    {
      Logger.AddInformation("Attempting to InitSebSettings", (object) null, (Exception) null, (string) null);
      if (!SebWindowsClientMain._loadingSebFile && !SebWindowsClientMain.clientSettingsSet)
      {
        if (!SEBClientInfo.SetSebClientConfiguration())
        {
          int num = (int) SEBMessageBox.Show(SEBUIStrings.ErrorCaption, SEBUIStrings.ErrorWhenOpeningSettingsFile, MessageBoxIcon.Hand, MessageBoxButtons.OK, false);
          Logger.AddError("Error when opening the file SebClientSettings.seb!", (object) null, (Exception) null, (string) null);
          return false;
        }
        SebWindowsClientMain.clientSettingsSet = true;
        Logger.AddError("SEB client configuration set in InitSebSettings().", (object) null, (Exception) null, (string) null);
      }
      if (!SEBClientInfo.SetSystemVersionInfo())
      {
        int num = (int) SEBMessageBox.Show(SEBUIStrings.ErrorCaption, SEBUIStrings.OSNotSupported, MessageBoxIcon.Hand, MessageBoxButtons.OK, false);
        Logger.AddError("Unknown OS. Exiting SEB.", (object) null, (Exception) null, (string) null);
        return false;
      }
      /*if (SEBClientInfo.IsNewOS)
      {
        SebWindowsClientMain.sessionCreateNewDesktop = (bool) SEBClientInfo.getSebSetting("createNewDesktop")["createNewDesktop"];
        if (SebWindowsClientMain.sessionCreateNewDesktop)
        {
          SEBClientInfo.OriginalDesktop = SEBDesktopController.GetCurrent();
          SEBDesktopController.OpenInputDesktop();
          SEBClientInfo.SEBNewlDesktop = SEBDesktopController.CreateDesktop("SEBDesktop");
          SEBDesktopController.Show(SEBClientInfo.SEBNewlDesktop.DesktopName);
          if (!SEBDesktopController.SetCurrent(SEBClientInfo.SEBNewlDesktop))
          {
            Logger.AddError("SetThreadDesktop failed! Looks like the thread has hooks or windows in the current desktop.", (object) null, (Exception) null, (string) null);
            SEBDesktopController.Show(SEBClientInfo.OriginalDesktop.DesktopName);
            SEBDesktopController.SetCurrent(SEBClientInfo.OriginalDesktop);
            SEBClientInfo.SEBNewlDesktop.Close();
            int num = (int) SEBMessageBox.Show(SEBUIStrings.createNewDesktopFailed, SEBUIStrings.createNewDesktopFailedReason, MessageBoxIcon.Hand, MessageBoxButtons.OK, false);
            return false;
          }
          SEBClientInfo.DesktopName = "SEBDesktop";
        }
        else
        {
          SEBClientInfo.OriginalDesktop = SEBDesktopController.GetCurrent();
          SEBClientInfo.DesktopName = SEBClientInfo.OriginalDesktop.DesktopName;
        }
      }*/
      Logger.AddInformation("Successfully InitSebSettings", (object) null, (Exception) null, (string) null);
      return true;
    }

    public static bool InitSEBDesktop()
    {
      Logger.AddInformation("Attempting to InitSEBDesktop", (object) null, (Exception) null, (string) null);
      SEBDesktopWallpaper.BlankWallpaper();
      SEBClipboard.CleanClipboard();
      Logger.AddInformation("Clipboard cleaned.", (object) null, (Exception) null, (string) null);
      SEBWindowHandler.AllowedExecutables.Clear();
      SEBWindowHandler.AllowedExecutables.Add("safeexambrowser");
      foreach (Dictionary<string, object> permittedProcess in SEBSettings.permittedProcessList)
      {
        if ((bool) permittedProcess["active"])
        {
          SEBWindowHandler.AllowedExecutables.Add(((string) permittedProcess["executable"]).ToLower());
          if (!string.IsNullOrWhiteSpace(permittedProcess["windowHandlingProcess"].ToString()))
            SEBWindowHandler.AllowedExecutables.Add(((string) permittedProcess["windowHandlingProcess"]).ToLower());
        }
      }
      if ((bool) SEBClientInfo.getSebSetting("monitorProcesses")["monitorProcesses"])
      {
        try
        {
          SEBWindowHandler.EnableForegroundWatchDog();
        }
        catch (Exception ex)
        {
          Logger.AddError("Unable to EnableForegroundWatchDog", (object) null, ex, (string) null);
        }
        SEBProcessHandler.ProhibitedExecutables.Clear();
        foreach (Dictionary<string, object> prohibitedProcess in SEBSettings.prohibitedProcessList)
        {
          if ((bool) prohibitedProcess["active"])
            SEBProcessHandler.ProhibitedExecutables.Add(((string) prohibitedProcess["executable"]).ToLower());
        }
        try
        {
          SEBProcessHandler.EnableProcessWatchDog();
        }
        catch (Exception ex)
        {
          Logger.AddError("Unable to EnableProcessWatchDog", (object) null, ex, (string) null);
        }
      }
      SEBClientInfo.ExplorerShellWasKilled = false;
      if ((bool) SEBClientInfo.getSebSetting("killExplorerShell")["killExplorerShell"])
      {
        try
        {
          SEBWindowHandler.MinimizeAllOpenWindows(false);
        }
        catch (Exception ex)
        {
          Logger.AddError("Unable to MinimizeAllOpenWindows", (object) null, ex, (string) null);
        }
        try
        {
          SEBClientInfo.ExplorerShellWasKilled = SEBProcessHandler.KillExplorerShell();
        }
        catch (Exception ex)
        {
          Logger.AddError("Unable to KillExplorerShell", (object) null, ex, (string) null);
        }
      }
      Logger.AddInformation("Successfully InitSEBDesktop", (object) null, (Exception) null, (string) null);
      return true;
    }

    public static void ResetSEBDesktop()
    {
      if (!SebWindowsClientMain.sessionCreateNewDesktop)
        return;
      Logger.AddInformation("Showing Original Desktop", (object) null, (Exception) null, (string) null);
      SEBDesktopController.Show(SEBClientInfo.OriginalDesktop.DesktopName);
      Logger.AddInformation("Setting original Desktop as current", (object) null, (Exception) null, (string) null);
      SEBDesktopController.SetCurrent(SEBClientInfo.OriginalDesktop);
      Logger.AddInformation("Closing New Dekstop", (object) null, (Exception) null, (string) null);
      SEBClientInfo.SEBNewlDesktop.Close();
    }

    public static void SetVisibility(bool show)
    {
      IntPtr window = SebWindowsClientMain.FindWindow("Shell_TrayWnd", (string) null);
      IntPtr hwnd = SebWindowsClientMain.FindWindowEx(window, IntPtr.Zero, "Button", "Start");
      if (hwnd == IntPtr.Zero)
        hwnd = SebWindowsClientMain.FindWindowEx(IntPtr.Zero, IntPtr.Zero, (IntPtr) 49175, "Start");
      if (hwnd == IntPtr.Zero)
      {
        hwnd = SebWindowsClientMain.FindWindow("Button", (string) null);
        if (hwnd == IntPtr.Zero)
          hwnd = SebWindowsClientMain.GetVistaStartMenuWnd(window);
      }
      SebWindowsClientMain.ShowWindow(window, show ? 5 : 0);
      SebWindowsClientMain.ShowWindow(hwnd, show ? 5 : 0);
    }

    private static IntPtr GetVistaStartMenuWnd(IntPtr taskBarWnd)
    {
      int lpdwProcessId;
      int windowThreadProcessId = (int) SebWindowsClientMain.GetWindowThreadProcessId(taskBarWnd, out lpdwProcessId);
      Process processById = Process.GetProcessById(lpdwProcessId);
      if (processById != null)
      {
        foreach (ProcessThread thread in (ReadOnlyCollectionBase) processById.Threads)
          SebWindowsClientMain.EnumThreadWindows(thread.Id, new SebWindowsClientMain.EnumThreadProc(SebWindowsClientMain.MyEnumThreadWindowsProc), IntPtr.Zero);
      }
      return SebWindowsClientMain.vistaStartMenuWnd;
    }

    private static bool MyEnumThreadWindowsProc(IntPtr hWnd, IntPtr lParam)
    {
      StringBuilder text = new StringBuilder(256);
      if (SebWindowsClientMain.GetWindowText(hWnd, text, text.Capacity) > 0)
      {
        Console.WriteLine((object) text);
        if (text.ToString() == "Start")
        {
          SebWindowsClientMain.vistaStartMenuWnd = hWnd;
          return false;
        }
      }
      return true;
    }

    public static void CheckIfInsideVirtualMachine()
    {
      if (SebWindowsClientMain.IsInsideVM() && !(bool) SEBClientInfo.getSebSetting("allowVirtualMachine")["allowVirtualMachine"])
      {
        int num = (int) SEBMessageBox.Show(SEBUIStrings.detectedVirtualMachine, SEBUIStrings.detectedVirtualMachineForbiddenMessage, MessageBoxIcon.Hand, MessageBoxButtons.OK, false);
        Logger.AddError("Forbidden to run SEB on a virtual machine!", (object) null, (Exception) null, (string) null);
        Logger.AddInformation("Safe Exam Browser is exiting", (object) null, (Exception) null, (string) null);
        throw new SEBNotAllowedToRunEception("Forbidden to run SEB on a virtual machine!");
      }
    }

    public static void CheckServicePolicy(bool isServiceAvailable)
    {
      switch ((int) SEBClientInfo.getSebSetting("sebServicePolicy")["sebServicePolicy"])
      {
        case 1:
          if (isServiceAvailable)
            break;
          int num1 = (int) SEBMessageBox.Show(SEBUIStrings.indicateMissingService, SEBUIStrings.indicateMissingServiceReason, MessageBoxIcon.Hand, MessageBoxButtons.OK, false);
          break;
        case 2:
          if (isServiceAvailable)
            break;
          int num2 = (int) SEBMessageBox.Show(SEBUIStrings.indicateMissingService, SEBUIStrings.forceSebServiceMessage, MessageBoxIcon.Hand, MessageBoxButtons.OK, false);
          Logger.AddError("SEB Windows service is not available and sebServicePolicies is set to forceSebService", (object) null, (Exception) null, (string) null);
          Logger.AddInformation("SafeExamBrowser is exiting", (object) null, (Exception) null, (string) null);
          throw new SEBNotAllowedToRunEception("SEB Windows service is not available and sebServicePolicies is set to forceSebService");
      }
    }

    public static void CheckDistributionIntegrity()
    {
      string path1 = SEBXulRunnerSettings.ResolveResourcePath((string) SEBSettings.valueForDictionaryKey(SEBSettings.settingsCurrent, "startURL"));
      string path2 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "SafeExamBrowser", "SebClientSettings.seb");
      if (!File.Exists(path1))
        throw new Exception(string.Format("Distribution is corrupted, file \"{0}\" isn't exists, it's insecure to run app now.", (object) path1));
      if (!File.Exists(path2))
        throw new Exception(string.Format("Distribution is corrupted, file \"{0}\" isn't exists, it's insecure to run app now.", (object) path2));
      byte[] array = ((IEnumerable<byte>) File.ReadAllBytes(path1)).Concat<byte>((IEnumerable<byte>) File.ReadAllBytes(path2)).ToArray<byte>();
      string base64String;
      using (MD5 md5 = MD5.Create())
        base64String = Convert.ToBase64String(md5.ComputeHash(array));
      if (base64String != IntegrityConstants.IndexCRC)
        throw new Exception("Distribution was changed externally, it's insecure to run app now.");
    }

    public static void SEBToForeground()
    {
      try
      {
        SebApplicationChooserForm.forceSetForegroundWindow(SEBClientInfo.SebWindowsClientForm.Handle);
        SEBClientInfo.SebWindowsClientForm.Activate();
      }
      catch (Exception ex)
      {
      }
    }

    public static void SetProcessLandscapePreference()
    {
      IntPtr hModule = SebWindowsClientMain.LoadLibrary("user32.dll");
      if (hModule == IntPtr.Zero)
        return;
      try
      {
        IntPtr procAddress = SebWindowsClientMain.GetProcAddress(hModule, "SetDisplayAutoRotationPreferences");
        if (procAddress == IntPtr.Zero)
          Logger.AddInformation("SetProcessLandscapePreference - Unsupported OS detected.", (object) null, (Exception) null, (string) null);
        else
          Logger.AddInformation(string.Format("SetProcessLandscapePreference(Landscape) - {0}", (object) ((SebWindowsClientMain.SetDisplayAutoRotationPreferences) Marshal.GetDelegateForFunctionPointer(procAddress, typeof (SebWindowsClientMain.SetDisplayAutoRotationPreferences)))(5)), (object) null, (Exception) null, (string) null);
      }
      finally
      {
        SebWindowsClientMain.FreeLibrary(hModule);
      }
    }

    private delegate bool EnumThreadProc(IntPtr hwnd, IntPtr lParam);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate bool SetDisplayAutoRotationPreferences(int OrientationPreference);
  }
}
