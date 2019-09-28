

using Microsoft.Win32;
using SebWindowsClient.BlockShortcutsUtils;
using SebWindowsClient.ConfigurationUtils;
using SebWindowsClient.DesktopUtils;
using SebWindowsClient.DiagnosticsUtils;
using SebWindowsClient.ProcessUtils;
using SebWindowsClient.ServiceUtils;
using SebWindowsClient.UI;
using SebWindowsClient.XULRunnerCommunication;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace SebWindowsClient
{
  public class SebWindowsClientForm : Form
  {
    private static IntPtr vistaStartMenuWnd = IntPtr.Zero;
    private static IntPtr processWindowHandle = IntPtr.Zero;
    public bool closeSebClient = true;
    public Process xulRunner = new Process();
    public List<string> permittedProcessesCalls = new List<string>();
    public List<Process> permittedProcessesReferences = new List<Process>();
    public List<Image> permittedProcessesIconImages = new List<Image>();
    private List<Process> runningProcessesToClose = new List<Process>();
    private List<string> runningApplicationsToClose = new List<string>();
    private readonly Form _orientationLockForm = (Form) new OrientationLockForm();
    private float scaleFactor = 1f;
    private const int SW_HIDE = 0;
    private const int SW_SHOW = 5;
    private const int SW_RESTORE = 9;
    private const int NOTIFY_FOR_THIS_SESSION = 0;
    private const int NOTIFY_FOR_ALL_SESSIONS = 1;
    private const int WM_WTSSESSION_CHANGE = 689;
    private const string VistaStartMenuCaption = "Start";
    private int taskbarHeight;
    public string sebPassword;
    private SebCloseDialogForm sebCloseDialogForm;
    private SebApplicationChooserForm sebApplicationChooserForm;
    private int xulRunnerExitCode;
    private DateTime xulRunnerExitTime;
    private bool closeDialogConfirmationIsOpen;
    private IContainer components;
    private ImageList ilProcessIcons;
    private TaskbarToolStrip taskbarToolStrip;

    [DllImport("user32.dll")]
    private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

    [DllImport("user32.dll")]
    private static extern IntPtr GetDesktopWindow();

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern bool EnumThreadWindows(int threadId, SebWindowsClientForm.EnumThreadProc pfnEnum, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowTitle);

    [DllImport("user32.dll")]
    private static extern IntPtr FindWindowEx(IntPtr parentHwnd, IntPtr childAfterHwnd, IntPtr className, string windowText);

    [DllImport("User32.dll")]
    private static extern bool IsIconic(IntPtr handle);

    [DllImport("user32.dll")]
    private static extern int ShowWindow(IntPtr hwnd, int nCmdShow);

    [DllImport("User32.dll")]
    private static extern IntPtr SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hwnd, out int lpdwProcessId);

    [DllImport("WtsApi32.dll")]
    private static extern bool WTSRegisterSessionNotification(IntPtr hWnd, [MarshalAs(UnmanagedType.U4)] int dwFlags);

    [DllImport("WtsApi32.dll")]
    private static extern bool WTSUnRegisterSessionNotification(IntPtr hWnd);

    public SebWindowsClientForm()
    {
      this.InitializeComponent();
      SEBXULRunnerWebSocketServer.OnXulRunnerCloseRequested += new EventHandler(this.OnXULRunnerShutdDownRequested);
      SEBXULRunnerWebSocketServer.OnXulRunnerQuitLinkClicked += new EventHandler(this.OnXulRunnerQuitLinkPressed);
      SystemEvents.DisplaySettingsChanged += (EventHandler) ((x, y) => this.PlaceFormOnDesktop(TapTipHandler.IsKeyboardVisible(), false));
      IntPtr handle = this.Handle;
      if (!SebWindowsClientForm.WTSRegisterSessionNotification(this.Handle, 0))
        Logger.AddError("Could not register for session events!", (object) null, (Exception) null, (string) null);
      try
      {
        SEBProcessHandler.PreventSleep();
      }
      catch (Exception ex)
      {
        Logger.AddError("Unable to PreventSleep", (object) null, ex, (string) null);
      }
    }

    protected override void OnHandleCreated(EventArgs e)
    {
      base.OnHandleCreated(e);
      OrientationProvider.Changed += new EventHandler<OrientationProvider.Orientation>(this.OrientationProvider_Changed);
      this.OrientationProvider_Changed((object) null, OrientationProvider.Current);
    }

    private void OrientationProvider_Changed(object sender, OrientationProvider.Orientation orientation)
    {
      if (orientation != OrientationProvider.Orientation.Portrait)
        return;
      this.BeginInvoke((Delegate) new MethodInvoker(this.ShowOrientationLockForm));
    }

    private void ShowOrientationLockForm()
    {
      OrientationProvider.Changed -= new EventHandler<OrientationProvider.Orientation>(this.OrientationProvider_Changed);
      int num = (int) this._orientationLockForm.ShowDialog((IWin32Window) this);
      OrientationProvider.Changed += new EventHandler<OrientationProvider.Orientation>(this.OrientationProvider_Changed);
    }

    private void OnXULRunnerShutdDownRequested(object sender, EventArgs e)
    {
      if (!(bool) SEBSettings.settingsCurrent["allowQuit"])
        return;
      Logger.AddInformation("Receiving Shutdown Request and opening ShowCloseDialogForm", (object) null, (Exception) null, (string) null);
      this.BeginInvoke((Delegate) new Action(this.ShowCloseDialogForm));
    }

    private void OnXulRunnerQuitLinkPressed(object sender, EventArgs e)
    {
      Logger.AddInformation("Receiving Quit Link pressed and opening ShowCloseDialogForm", (object) null, (Exception) null, (string) null);
      this.BeginInvoke((Delegate) new Action(this.ShowCloseDialogFormConfirmation));
    }

    protected override void OnLoad(EventArgs e)
    {
      base.OnLoad(e);
      string[] commandLineArgs = Environment.GetCommandLineArgs();
      Logger.AddError("OnLoad EventArgs: " + string.Join(", ", commandLineArgs), (object) null, (Exception) null, (string) null);
      if (commandLineArgs.Length <= 1 || this.LoadFile(commandLineArgs[1]))
        return;
      Logger.AddError("LoadFile() in OnLoad() failed, exiting SEB!", (object) null, (Exception) null, (string) null);
      this.ExitApplication(true);
    }

    public bool LoadFile(string file)
    {
      Logger.AddInformation("Attempting to read new configuration file", (object) null, (Exception) null, (string) null);
      if (SebWindowsClientMain.isLoadingSebFile())
        return false;
      SebWindowsClientMain.LoadingSebFile(true);
      if (!SebWindowsClientMain.clientSettingsSet && SEBClientInfo.SetSebClientConfiguration())
      {
        SebWindowsClientMain.clientSettingsSet = true;
        Logger.AddError("SEB client configuration set in LoadFile(URI).", (object) null, (Exception) null, (string) null);
      }
      byte[] sebData = (byte[]) null;
      Uri uri;
      try
      {
        uri = new Uri(file);
      }
      catch (Exception ex)
      {
        Logger.AddError("SEB was opened with a wrong URI parameter", (object) this, ex, ex.Message);
        SebWindowsClientMain.LoadingSebFile(false);
        return false;
      }
      if (SEBClientInfo.examMode)
      {
        int num = (int) SEBMessageBox.Show(SEBUIStrings.loadingSettingsNotAllowed, SEBUIStrings.loadingSettingsNotAllowedReason, MessageBoxIcon.Hand, MessageBoxButtons.OK, false);
        SebWindowsClientMain.LoadingSebFile(false);
        return false;
      }
      if (uri.Scheme == "seb" || uri.Scheme == "sebs")
      {
        if ((bool) SEBSettings.valueForDictionaryKey(SEBSettings.settingsCurrent, "downloadAndOpenSebConfig"))
        {
          try
          {
            WebClient webClient = new WebClient();
            if (uri.Scheme == "seb")
            {
              Logger.AddError("Trying to download .seb settings by http", (object) null, (Exception) null, (string) null);
              UriBuilder uriBuilder = new UriBuilder("http", uri.Host, uri.Port, uri.AbsolutePath);
              using (webClient)
                sebData = webClient.DownloadData(uriBuilder.Uri);
              if (sebData == null)
                Logger.AddError("Downloading .seb settings by http failed, try to download by https", (object) null, (Exception) null, (string) null);
            }
            if (sebData == null)
            {
              Logger.AddError("Downloading .seb settings by https", (object) null, (Exception) null, (string) null);
              UriBuilder uriBuilder = new UriBuilder("https", uri.Host, uri.Port, uri.AbsolutePath);
              using (webClient)
                sebData = webClient.DownloadData(uriBuilder.Uri);
            }
          }
          catch (Exception ex)
          {
            int num = (int) SEBMessageBox.Show(SEBUIStrings.cannotOpenSEBLink, SEBUIStrings.cannotOpenSEBLinkMessage, MessageBoxIcon.Hand, MessageBoxButtons.OK, false);
            Logger.AddError("Unable to download a file from the " + file + " link", (object) this, ex, (string) null);
          }
        }
      }
      else if (uri.IsFile)
      {
        try
        {
          sebData = System.IO.File.ReadAllBytes(file);
        }
        catch (Exception ex)
        {
          Logger.AddError("Settings could not be read from file.", (object) this, ex, ex.Message);
          SebWindowsClientMain.LoadingSebFile(false);
          return false;
        }
      }
      if (sebData == null)
      {
        Logger.AddError("Loaded settings were empty.", (object) this, (Exception) null, (string) null);
        SebWindowsClientMain.LoadingSebFile(false);
        return false;
      }
      Logger.AddInformation("Succesfully read the new configuration, length is " + (object) sebData.Length, (object) null, (Exception) null, (string) null);
      Logger.AddInformation("Attempting to StoreDecryptedSEBSettings", (object) null, (Exception) null, (string) null);
      if (!SEBConfigFileManager.StoreDecryptedSEBSettings(sebData))
      {
        Logger.AddInformation("StoreDecryptedSettings returned false, this means the user canceled when entering the password, didn't enter a right one after 5 attempts or new settings were corrupted, exiting", (object) null, (Exception) null, (string) null);
        Logger.AddError("Settings could not be decrypted or stored.", (object) this, (Exception) null, (string) null);
        SebWindowsClientMain.LoadingSebFile(false);
        return false;
      }
      Thread thread = new Thread(new ThreadStart(SEBSplashScreen.StartSplash));
      if (!SEBXULRunnerWebSocketServer.Started)
      {
        Logger.AddInformation("SEBXULRunnerWebSocketServer.Started returned false, this means the WebSocketServer communicating with the SEB XULRunner browser couldn't be started, exiting", (object) null, (Exception) null, (string) null);
        int num = (int) SEBMessageBox.Show(SEBUIStrings.webSocketServerNotStarted, SEBUIStrings.webSocketServerNotStartedMessage, MessageBoxIcon.Hand, MessageBoxButtons.OK, false);
        this.ExitApplication(true);
        return false;
      }
      SEBSplashScreen.CloseSplash();
      Logger.AddInformation("Successfully StoreDecryptedSEBSettings", (object) null, (Exception) null, (string) null);
      SebWindowsClientMain.LoadingSebFile(false);
      return true;
    }

    public static void SEBToForeground()
    {
      try
      {
        SebWindowsClientForm.SetForegroundWindow(SEBClientInfo.SebWindowsClientForm.Handle);
        SEBClientInfo.SebWindowsClientForm.Activate();
      }
      catch (Exception ex)
      {
      }
    }

    private bool StartXulRunner(string userDefinedArguments)
    {
      string path = "";
      string desktop = "";
      if (userDefinedArguments == null)
        userDefinedArguments = "";
      try
      {
        Dictionary<string, object> settingsCurrent = SEBSettings.settingsCurrent;
        Func<KeyValuePair<string, object>, string> func = (Func<KeyValuePair<string, object>, string>) (entry => entry.Key);
        Func<KeyValuePair<string, object>, string> keySelector = null;
        string str1 = SEBXulRunnerSettings.XULRunnerConfigDictionarySerialize(settingsCurrent.ToDictionary<KeyValuePair<string, object>, string, object>(keySelector, (Func<KeyValuePair<string, object>, object>) (entry => entry.Value)));
        StringBuilder stringBuilder1 = new StringBuilder(SEBClientInfo.XulRunnerExePath);
        StringBuilder stringBuilder2 = new StringBuilder(" -app \"").Append(Application.StartupPath).Append("\\").Append(SEBClientInfo.XulRunnerSebIniPath).Append("\"");
        if (!userDefinedArguments.ToLower().Contains("-profile"))
          stringBuilder2.Append(" -profile \"").Append(SEBClientInfo.SebClientSettingsAppDataDirectory).Append("Profiles\"");
        if (!userDefinedArguments.ToLower().Contains("-logfile") && (bool) SEBSettings.valueForDictionaryKey(SEBSettings.settingsCurrent, "enableLogging"))
        {
          string name = (string) SEBSettings.valueForDictionaryKey(SEBSettings.settingsCurrent, "logDirectoryWin");
          if (string.IsNullOrEmpty(name))
          {
            stringBuilder2.Append(" -logfile \"").Append(SEBClientInfo.SebClientSettingsAppDataDirectory).Append("\\seb.log\"");
          }
          else
          {
            string str2 = Environment.ExpandEnvironmentVariables(name);
            stringBuilder2.Append(" -logfile \"").Append(str2).Append("\\seb.log\"");
          }
          if (!userDefinedArguments.ToLower().Contains("-debug"))
            stringBuilder2.Append(" -debug 1");
        }
        stringBuilder2.Append(" ").Append(Environment.ExpandEnvironmentVariables(userDefinedArguments)).Append(" –purgecaches -ctrl \"").Append(str1).Append("\"");
        string str3 = stringBuilder2.ToString();
        stringBuilder1.Append(str3);
        path = stringBuilder1.ToString();
        desktop = SEBClientInfo.DesktopName;
        this.xulRunner = SEBDesktopController.CreateProcess(path, desktop);
        this.xulRunner.EnableRaisingEvents = true;
        this.xulRunner.Exited += new EventHandler(this.xulRunner_Exited);
        return true;
      }
      catch (Exception ex)
      {
        Logger.AddError("An error occurred starting XULRunner, path: " + path + " desktop name: " + desktop + " ", (object) this, ex, ex.Message);
        return false;
      }
    }

    private void xulRunner_Exited(object sender, EventArgs e)
    {
      Logger.AddError("XULRunner exit event fired.", (object) this, (Exception) null, (string) null);
      if (this.xulRunner != null)
      {
        try
        {
          this.xulRunnerExitCode = this.xulRunner.ExitCode;
          this.xulRunnerExitTime = this.xulRunner.ExitTime;
        }
        catch (Exception ex)
        {
          this.xulRunnerExitCode = -1;
          Logger.AddError("Error reading XULRunner exit code!", (object) this, ex, (string) null);
        }
      }
      else
        this.xulRunnerExitCode = 0;
      if (this.xulRunnerExitCode != 0)
      {
        Logger.AddError("An error occurred when exiting XULRunner. Exit code: " + this.xulRunnerExitCode.ToString(), (object) this, (Exception) null, (string) null);
      }
      else
      {
        if (!SEBClientInfo.SebWindowsClientForm.closeSebClient)
          return;
        Logger.AddError("XULRunner was closed, SEB will exit now.", (object) this, (Exception) null, (string) null);
        this.ExitApplication(true);
      }
    }

    private void addPermittedProcessesToTS()
    {
      this.taskbarToolStrip.Items.Clear();
      this.permittedProcessesCalls.Clear();
      this.permittedProcessesReferences.Clear();
      this.permittedProcessesIconImages.Clear();
      List<object> objectList1 = (List<object>) SEBClientInfo.getSebSetting("permittedProcesses")["permittedProcesses"];
      if (objectList1.Count > 0)
      {
        List<Process> processList = new List<Process>();
        List<Process> list = ((IEnumerable<Process>) Process.GetProcesses()).ToList<Process>();
        for (int index1 = 0; index1 < objectList1.Count; ++index1)
        {
          Dictionary<string, object> dictionary = (Dictionary<string, object>) objectList1[index1];
          if (!(bool) SEBSettings.valueForDictionaryKey(dictionary, "runInBackground") && (SEBSettings.operatingSystems) SEBSettings.valueForDictionaryKey(dictionary, "os") == SEBSettings.operatingSystems.operatingSystemWin & (bool) SEBSettings.valueForDictionaryKey(dictionary, "active"))
          {
            string str1 = (string) SEBSettings.valueForDictionaryKey(dictionary, "title");
            string str2 = (string) dictionary["executable"];
            if (string.IsNullOrEmpty(str1))
              str1 = str2;
            string title = (string) dictionary["identifier"];
            if (!str2.Contains("xulrunner.exe") || (bool) SEBSettings.valueForDictionaryKey(SEBSettings.settingsCurrent, "enableSebBrowser"))
            {
              int index2 = 0;
              while (index2 < list.Count<Process>())
              {
                try
                {
                  string processName = list[index2].ProcessName;
                  if (str2.Contains(processName))
                  {
                    Process process = list[index2];
                    if (process != null && !process.HasExited && process.MainWindowHandle == IntPtr.Zero)
                      process = SEBWindowHandler.GetWindowHandleByTitle(title).GetProcess();
                    if ((bool) SEBSettings.valueForDictionaryKey(dictionary, "strongKill"))
                    {
                      Logger.AddError("Closing already running permitted process with strongKill flag set: " + processName, (object) null, (Exception) null, (string) null);
                      SEBNotAllowedProcessController.CloseProcess(process);
                      list.RemoveAt(index2);
                    }
                    else
                    {
                      this.runningProcessesToClose.Add(process);
                      this.runningApplicationsToClose.Add(str1 == "SEB" ? str2 : str1);
                      ++index2;
                    }
                  }
                  else
                    ++index2;
                }
                catch (Exception ex)
                {
                  list.RemoveAt(index2);
                }
              }
            }
          }
        }
      }
      if (this.runningProcessesToClose.Count > 0)
      {
        StringBuilder stringBuilder = new StringBuilder();
        foreach (string str in this.runningApplicationsToClose)
          stringBuilder.AppendLine("    " + str);
        if (SEBMessageBox.Show(SEBUIStrings.closeProcesses, SEBUIStrings.closeProcessesQuestion + "\n\n" + stringBuilder.ToString(), MessageBoxIcon.Hand, MessageBoxButtons.OKCancel, false) == DialogResult.OK)
        {
          foreach (Process processToClose in this.runningProcessesToClose)
            SEBNotAllowedProcessController.CloseProcess(processToClose);
          this.runningProcessesToClose.Clear();
          this.runningApplicationsToClose.Clear();
        }
        else
        {
          this.ExitApplication(true);
          return;
        }
      }
      if (objectList1.Count > 0)
      {
        for (int index1 = 0; index1 < objectList1.Count; ++index1)
        {
          Dictionary<string, object> dictionary1 = (Dictionary<string, object>) objectList1[index1];
          if (!(bool) SEBSettings.valueForDictionaryKey(dictionary1, "runInBackground") || (bool) SEBSettings.valueForDictionaryKey(dictionary1, "autostart"))
          {
            int num1 = (int) SEBSettings.valueForDictionaryKey(dictionary1, "os");
            bool flag = (bool) SEBSettings.valueForDictionaryKey(dictionary1, "active");
            int num2 = 1;
            if (num1 == num2 & flag)
            {
              string str1 = (string) SEBSettings.valueForDictionaryKey(dictionary1, "identifier");
              string str2 = (string) SEBSettings.valueForDictionaryKey(dictionary1, "windowHandlingProcess");
              string newValue = (string) SEBSettings.valueForDictionaryKey(dictionary1, "title");
              string str3 = (string) dictionary1["executable"];
              if (string.IsNullOrEmpty(newValue))
                newValue = str3;
              if (!str3.Contains("xulrunner.exe") || (bool) SEBSettings.valueForDictionaryKey(SEBSettings.settingsCurrent, "enableSebBrowser"))
              {
                SEBToolStripButton sebToolStripButton = new SEBToolStripButton();
                if (!(bool) SEBSettings.valueForDictionaryKey(dictionary1, "iconInTaskbar"))
                  sebToolStripButton.Visible = false;
                sebToolStripButton.Padding = new Padding(5, 0, 5, 0);
                sebToolStripButton.ToolTipText = newValue;
                sebToolStripButton.Identifier = str1;
                sebToolStripButton.WindowHandlingProcess = str2;
                string str4 = !str3.Contains("xulrunner.exe") ? this.GetPermittedApplicationPath(dictionary1) : Application.ExecutablePath;
                if (str4 != null)
                {
                  int height = sebToolStripButton.Height;
                  Bitmap highResIconImage = Iconextractor.ExtractHighResIconImage(str4, new int?(this.taskbarHeight - 8));
                  if (highResIconImage == null)
                  {
                    Icon applicationIcon = this.GetApplicationIcon(str4);
                    if (applicationIcon == null && highResIconImage == null)
                      applicationIcon = this.GetApplicationIcon(str4);
                    if (applicationIcon == null)
                      applicationIcon = this.GetApplicationIcon(Application.ExecutablePath);
                    sebToolStripButton.Image = (Image) applicationIcon.ToBitmap();
                  }
                  else
                    sebToolStripButton.Image = (Image) highResIconImage;
                  this.permittedProcessesIconImages.Add(sebToolStripButton.Image);
                  sebToolStripButton.Click += new EventHandler(this.ToolStripButton_Click);
                  sebToolStripButton.Name = this.permittedProcessesCalls.Count.ToString();
                  this.taskbarToolStrip.Items.Add((ToolStripItem) sebToolStripButton);
                  if (!str3.Contains("xulrunner.exe"))
                  {
                    StringBuilder stringBuilder = new StringBuilder(str4);
                    List<object> objectList2 = (List<object>) dictionary1["arguments"];
                    for (int index2 = 0; index2 < objectList2.Count; ++index2)
                    {
                      Dictionary<string, object> dictionary2 = (Dictionary<string, object>) objectList2[index2];
                      if ((bool) dictionary2["active"])
                        stringBuilder.Append(" ").Append((string) dictionary2["argument"]);
                    }
                    this.permittedProcessesCalls.Add(stringBuilder.ToString());
                  }
                  else if ((bool) SEBSettings.valueForDictionaryKey(SEBSettings.settingsCurrent, "enableSebBrowser"))
                  {
                    StringBuilder stringBuilder = new StringBuilder("");
                    List<object> objectList2 = (List<object>) dictionary1["arguments"];
                    for (int index2 = 0; index2 < objectList2.Count; ++index2)
                    {
                      Dictionary<string, object> dictionary2 = (Dictionary<string, object>) objectList2[index2];
                      if ((bool) dictionary2["active"])
                      {
                        string str5 = (string) dictionary2["argument"];
                        if (!str5.Contains("-app") && !str5.Contains("-ctrl"))
                          stringBuilder.Append(" ").Append((string) dictionary2["argument"]);
                      }
                    }
                    this.permittedProcessesCalls.Add(stringBuilder.ToString());
                  }
                }
                else
                {
                  this.permittedProcessesCalls.Add((string) null);
                  int num3 = (int) SEBMessageBox.Show(SEBUIStrings.permittedApplicationNotFound, SEBUIStrings.permittedApplicationNotFoundMessage.Replace("%s", newValue), MessageBoxIcon.Hand, MessageBoxButtons.OK, false);
                }
              }
            }
          }
        }
      }
      if ((bool) SEBSettings.settingsCurrent["allowQuit"])
      {
        SEBQuitToolStripButton quitToolStripButton = new SEBQuitToolStripButton();
        quitToolStripButton.Click += (EventHandler) ((x, y) => this.ShowCloseDialogForm());
        this.taskbarToolStrip.Items.Add((ToolStripItem) quitToolStripButton);
      }
      try
      {
        if ((bool) SEBClientInfo.getSebSetting("allowWlan")["allowWlan"])
          this.taskbarToolStrip.Items.Add((ToolStripItem) new SEBWlanToolStripButton());
      }
      catch (Exception ex)
      {
        Logger.AddError("Unable to add WLANControl", (object) this, ex, (string) null);
      }
      if ((bool) SEBClientInfo.getSebSetting("touchOptimized")["touchOptimized"] && !(bool) SEBClientInfo.getSebSetting("createNewDesktop")["createNewDesktop"])
      {
        this.taskbarToolStrip.Items.Add((ToolStripItem) new SEBOnScreenKeyboardToolStripButton());
        TapTipHandler.RegisterXulRunnerEvents();
        TapTipHandler.OnKeyboardStateChanged += (TapTipHandler.KeyboardStateChangedEventHandler) (shown => this.BeginInvoke((Delegate) (Action)(() => this.PlaceFormOnDesktop(shown, false))));
      }
      if (!string.IsNullOrEmpty(SEBClientInfo.getSebSetting("restartExamURL")["restartExamURL"].ToString()) || (bool) SEBSettings.settingsCurrent["restartExamUseStartURL"])
        this.taskbarToolStrip.Items.Add((ToolStripItem) new SEBRestartExamToolStripButton());
      if ((bool) SEBClientInfo.getSebSetting("showReloadButton")["showReloadButton"])
        this.taskbarToolStrip.Items.Add((ToolStripItem) new SEBReloadBrowserToolStripButton());
      try
      {
        this.taskbarToolStrip.Items.Add((ToolStripItem) new SEBBatterylifeToolStripButton());
      }
      catch (Exception ex)
      {
        Logger.AddError("Unable to add the Batterystatuscontrol", (object) this, ex, (string) null);
      }
      if ((bool) SEBClientInfo.getSebSetting("showInputLanguage")["showInputLanguage"])
        this.taskbarToolStrip.Items.Add((ToolStripItem) new SEBInputLanguageToolStripButton());
      if ((bool) SEBClientInfo.getSebSetting("showTime")["showTime"])
        this.taskbarToolStrip.Items.Add((ToolStripItem) new SEBWatchToolStripButton());
      int index3 = 0;
      for (int index1 = 0; index1 < objectList1.Count; ++index1)
      {
        Dictionary<string, object> dictionary = (Dictionary<string, object>) objectList1[index1];
        if (!(bool) SEBSettings.valueForDictionaryKey(dictionary, "runInBackground") || (bool) SEBSettings.valueForDictionaryKey(dictionary, "autostart"))
        {
          int num1 = (int) SEBSettings.valueForDictionaryKey(dictionary, "os");
          bool flag = (bool) SEBSettings.valueForDictionaryKey(dictionary, "active");
          string str = (string) dictionary["executable"];
          int num2 = 1;
          if (num1 == num2 & flag)
          {
            if (!str.Contains("xulrunner.exe"))
            {
              Process process = (Process) null;
              if ((bool) dictionary["autostart"])
              {
                string permittedProcessesCall = this.permittedProcessesCalls[index3];
                process = permittedProcessesCall == null ? (Process) null : this.CreateProcessWithExitHandler(permittedProcessesCall);
              }
              this.permittedProcessesReferences.Add(process);
              ++index3;
            }
            else if ((bool) SEBSettings.valueForDictionaryKey(SEBSettings.settingsCurrent, "enableSebBrowser"))
            {
              this.StartXulRunner(this.permittedProcessesCalls[index3]);
              this.permittedProcessesReferences.Add(this.xulRunner);
              ++index3;
            }
          }
        }
      }
      SebWindowsClientForm.SEBToForeground();
    }

    private Icon GetProcessIcon(Process process)
    {
      try
      {
        return Icon.ExtractAssociatedIcon(process.MainModule.FileName);
      }
      catch (Exception ex)
      {
        return (Icon) null;
      }
    }

    private Icon GetApplicationIcon(string fullPath)
    {
      try
      {
        return Icon.ExtractAssociatedIcon(fullPath);
      }
      catch (Exception ex)
      {
        return (Icon) null;
      }
    }

    public string GetApplicationPath(string executable, string executablePath = "")
    {
      if (System.IO.File.Exists(executable))
        return executable;
      string str1 = SEBClientInfo.ProgramFilesX86Directory + "\\";
      if (System.IO.File.Exists(str1 + executable))
        return str1;
      string str2 = Environment.SystemDirectory + "\\";
      if (System.IO.File.Exists(str2 + executable))
        return str2;
      using (RegistryKey registryKey1 = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, ""))
      {
        string name = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\App Paths\\" + executable;
        using (RegistryKey registryKey2 = registryKey1.OpenSubKey(name))
        {
          if (registryKey2 == null)
            return (string) null;
          object obj = registryKey2.GetValue("Path");
          if (obj != null)
            return (string) obj;
        }
      }
      return (string) null;
    }

    public string GetPermittedApplicationPath(Dictionary<string, object> permittedProcess)
    {
      string str1 = (string) SEBSettings.valueForDictionaryKey(permittedProcess, "executable") ?? "";
      string str2 = (string) SEBSettings.valueForDictionaryKey(permittedProcess, "path") ?? "";
      bool flag = (bool) SEBSettings.valueForDictionaryKey(permittedProcess, "allowUserToChooseApp");
      if (str2 != "")
      {
        string path = str2 + "\\" + str1;
        if (System.IO.File.Exists(path))
          return path;
      }
      string applicationPath1 = this.GetApplicationPath(str1, "");
      if (applicationPath1 != null)
      {
        string path1 = applicationPath1 + str1;
        if (System.IO.File.Exists(path1))
          return path1;
        string path2 = applicationPath1 + str2 + "\\" + str1;
        if (System.IO.File.Exists(path2))
          return path2;
      }
      string str3 = (string) null;
      string applicationPath2 = this.GetApplicationPath(str2 + "\\" + str1, "");
      if (applicationPath2 != null)
        str3 = applicationPath2 + str2 + "\\" + str1;
      if (!(str3 == null & flag) || string.IsNullOrEmpty(str1))
        return str3;
      SebWindowsClientForm.SEBToForeground();
      return ThreadedDialog.ShowFileDialogForExecutable(str1);
    }

    protected void ToolStripButton_Click(object sender, EventArgs e)
    {
      SEBToolStripButton sebToolStripButton = sender as SEBToolStripButton;
      int int32 = Convert.ToInt32(sebToolStripButton.Name);
      Process process1 = this.permittedProcessesReferences[int32];
      if (this.xulRunner != null)
      {
        if (process1 == this.xulRunner)
        {
          try
          {
            if (this.xulRunner.HasExited)
            {
              this.StartXulRunner(this.permittedProcessesCalls[int32]);
              return;
            }
            process1.Refresh();
            Process process2 = process1;
            Rectangle bounds = ((ToolStripItem) sender).Bounds;
            int x = bounds.X;
            bounds = Screen.PrimaryScreen.Bounds;
            int top = bounds.Height - this.taskbarHeight;
            WindowChooser windowChooser = new WindowChooser(process2, x, top);
            return;
          }
          catch (Exception ex)
          {
            this.StartXulRunner(this.permittedProcessesCalls[int32]);
            return;
          }
        }
      }
      try
      {
        if (process1 == null || process1.HasExited)
        {
          Process processWithExitHandler = this.CreateProcessWithExitHandler(this.permittedProcessesCalls[int32]);
          this.permittedProcessesReferences[int32] = processWithExitHandler;
        }
        else
        {
          process1.Refresh();
          if (process1.MainWindowHandle == IntPtr.Zero && !string.IsNullOrWhiteSpace(sebToolStripButton.WindowHandlingProcess))
          {
            foreach (KeyValuePair<IntPtr, string> openWindow in (IEnumerable<KeyValuePair<IntPtr, string>>) SEBWindowHandler.GetOpenWindows())
            {
              Process process2 = openWindow.Key.GetProcess();
              if (sebToolStripButton.WindowHandlingProcess.ToLower().Contains(process2.GetExecutableName().ToLower()) || process2.GetExecutableName().ToLower().Contains(sebToolStripButton.WindowHandlingProcess.ToLower()))
              {
                process1 = process2;
                break;
              }
            }
          }
          if (process1.MainWindowHandle == IntPtr.Zero)
            process1 = SEBWindowHandler.GetWindowHandleByTitle(sebToolStripButton.Identifier).GetProcess();
          WindowChooser windowChooser = new WindowChooser(process1, ((ToolStripItem) sender).Bounds.X, Screen.PrimaryScreen.Bounds.Height - this.taskbarHeight);
        }
      }
      catch (Exception ex)
      {
        Logger.AddError("Error when trying to start permitted process by clicking in SEB taskbar: ", (object) null, ex, (string) null);
      }
    }

    private static bool EnumThreadCallback(IntPtr hWnd, IntPtr lParam)
    {
      if (SebWindowsClientForm.IsIconic(hWnd))
        SebWindowsClientForm.ShowWindow(hWnd, 9);
      SebWindowsClientForm.SetForegroundWindow(hWnd);
      return true;
    }

    private Process CreateProcessWithExitHandler(string fullPathArgumentsCall)
    {
      return SEBDesktopController.CreateProcess(fullPathArgumentsCall, SEBClientInfo.DesktopName);
    }

    private void permittedProcess_Exited(object sender, EventArgs e)
    {
    }

    private bool SetFormOnDesktop()
    {
      if (!(bool) SEBSettings.valueForDictionaryKey(SEBSettings.settingsCurrent, "showTaskBar"))
        return false;
      float dpiX;
      using (Graphics graphics = this.CreateGraphics())
        dpiX = graphics.DpiX;
      this.scaleFactor = dpiX / 96f;
      SEBClientInfo.scaleFactor = this.scaleFactor;
      Logger.AddInformation("Current display DPI setting: " + dpiX.ToString() + " and scale factor: " + this.scaleFactor.ToString(), (object) null, (Exception) null, (string) null);
      float num = (float) (int) SEBSettings.valueForDictionaryKey(SEBSettings.settingsCurrent, "taskBarHeight");
      if ((bool) SEBClientInfo.getSebSetting("touchOptimized")["touchOptimized"])
      {
        this.taskbarHeight = (int) ((double) num * 1.7 * (double) this.scaleFactor);
        this.taskbarToolStrip.ImageScalingSize = new Size(this.taskbarHeight - 8, this.taskbarHeight - 8);
      }
      else
      {
        this.taskbarHeight = (int) ((double) num * (double) this.scaleFactor);
        this.taskbarToolStrip.ImageScalingSize = new Size(this.taskbarHeight - 8, this.taskbarHeight - 8);
      }
      Logger.AddInformation("Taskbarheight from settings: " + num.ToString() + " Current taskbar height: " + this.taskbarHeight.ToString(), (object) null, (Exception) null, (string) null);
      SEBWorkingAreaHandler.SetTaskBarSpaceHeight(this.taskbarHeight);
      this.FormBorderStyle = FormBorderStyle.None;
      SebWindowsClientForm.SetParent(this.Handle, SebWindowsClientForm.GetDesktopWindow());
      this.TopMost = true;
      this.PlaceFormOnDesktop(false, true);
      return true;
    }

    private void PlaceFormOnDesktop(bool KeyboardShown, bool isInitial = false)
    {
      if (KeyboardShown && TapTipHandler.IsKeyboardDocked() && (bool) SEBSettings.valueForDictionaryKey(SEBSettings.settingsCurrent, "touchOptimized"))
      {
        this.Hide();
        int windowHeight = TapTipHandler.GetKeyboardWindowHandle().GetWindowHeight();
        Logger.AddInformation("Keyboard height from its window: " + (object) windowHeight, (object) null, (Exception) null, (string) null);
        SEBWorkingAreaHandler.SetTaskBarSpaceHeight(windowHeight);
        KeyValuePair<IntPtr, string> keyValuePair = SEBWindowHandler.GetOpenWindows().FirstOrDefault<KeyValuePair<IntPtr, string>>();
        if (keyValuePair.Value != null)
          keyValuePair.Key.AdaptWindowToWorkingArea(new int?(windowHeight));
        SEBXULRunnerWebSocketServer.SendKeyboardShown();
      }
      else
      {
        SEBWorkingAreaHandler.SetTaskBarSpaceHeight(this.taskbarHeight);
        if ((bool) SEBSettings.valueForDictionaryKey(SEBSettings.settingsCurrent, "showTaskBar"))
        {
          Rectangle bounds = Screen.PrimaryScreen.Bounds;
          int width = bounds.Width;
          int x = 0;
          bounds = Screen.PrimaryScreen.Bounds;
          int y = bounds.Height - this.taskbarHeight;
          this.Height = this.taskbarHeight;
          this.Width = width;
          this.Location = new Point(x, y);
          this.Show();
        }
        if (!(bool) SEBSettings.valueForDictionaryKey(SEBSettings.settingsCurrent, "touchOptimized"))
          return;
        KeyValuePair<IntPtr, string> keyValuePair = SEBWindowHandler.GetOpenWindows().FirstOrDefault<KeyValuePair<IntPtr, string>>();
        if (keyValuePair.Value == null)
          return;
        if (isInitial)
        {
          foreach (KeyValuePair<IntPtr, string> openWindow in this.xulRunner.GetOpenWindows())
            openWindow.Key.MaximizeWindow();
        }
        else
          keyValuePair.Key.AdaptWindowToWorkingArea(new int?(this.taskbarHeight));
      }
    }

    private static IntPtr GetActiveWindowOfProcess(Process proc)
    {
      return SebWindowsClientForm.vistaStartMenuWnd;
    }

    private static bool MyEnumThreadWindowsForProcess(int pid, IntPtr lParam)
    {
      return true;
    }

    public static void SetVisibility(bool show)
    {
      IntPtr window = SebWindowsClientForm.FindWindow("Shell_TrayWnd", (string) null);
      IntPtr hwnd = SebWindowsClientForm.FindWindowEx(window, IntPtr.Zero, "Button", "Start");
      if (hwnd == IntPtr.Zero)
        hwnd = SebWindowsClientForm.FindWindowEx(IntPtr.Zero, IntPtr.Zero, (IntPtr) 49175, "Start");
      if (hwnd == IntPtr.Zero)
      {
        hwnd = SebWindowsClientForm.FindWindow("Button", (string) null);
        if (hwnd == IntPtr.Zero)
          hwnd = SebWindowsClientForm.GetVistaStartMenuWnd(window);
      }
      SebWindowsClientForm.ShowWindow(window, show ? 5 : 0);
      SebWindowsClientForm.ShowWindow(hwnd, show ? 5 : 0);
    }

    private static IntPtr GetVistaStartMenuWnd(IntPtr taskBarWnd)
    {
      int lpdwProcessId;
      int windowThreadProcessId = (int) SebWindowsClientForm.GetWindowThreadProcessId(taskBarWnd, out lpdwProcessId);
      Process processById = Process.GetProcessById(lpdwProcessId);
      if (processById != null)
      {
        foreach (ProcessThread thread in (ReadOnlyCollectionBase) processById.Threads)
          SebWindowsClientForm.EnumThreadWindows(thread.Id, new SebWindowsClientForm.EnumThreadProc(SebWindowsClientForm.MyEnumThreadWindowsProc), IntPtr.Zero);
      }
      return SebWindowsClientForm.vistaStartMenuWnd;
    }

    private static bool MyEnumThreadWindowsProc(IntPtr hWnd, IntPtr lParam)
    {
      StringBuilder text = new StringBuilder(256);
      if (SebWindowsClientForm.GetWindowText(hWnd, text, text.Capacity) > 0)
      {
        Console.WriteLine((object) text);
        if (text.ToString() == "Start")
        {
          SebWindowsClientForm.vistaStartMenuWnd = hWnd;
          return false;
        }
      }
      return true;
    }

    private bool InitClientRegistryAndKillProcesses()
    {
      List<object> source = (List<object>) SEBClientInfo.getSebSetting("prohibitedProcesses")["prohibitedProcesses"];
      if (source.Count<object>() > 0)
      {
        this.runningProcessesToClose.Clear();
        this.runningApplicationsToClose.Clear();
        for (int index1 = 0; index1 < source.Count; ++index1)
        {
          Dictionary<string, object> dictionary = (Dictionary<string, object>) source[index1];
          if ((SEBSettings.operatingSystems) SEBSettings.valueForDictionaryKey(dictionary, "os") == SEBSettings.operatingSystems.operatingSystemWin & (bool) SEBSettings.valueForDictionaryKey(dictionary, "active"))
          {
            string str1 = (string) SEBSettings.valueForDictionaryKey(dictionary, "title") ?? "";
            string str2 = (string) dictionary["executable"];
            Process[] processes = Process.GetProcesses();
            for (int index2 = 0; index2 < ((IEnumerable<Process>) processes).Count<Process>(); ++index2)
            {
              string processName = processes[index2].ProcessName;
              if (processName != null && str2.Contains(processName))
              {
                if ((bool) SEBSettings.valueForDictionaryKey(dictionary, "strongKill"))
                {
                  SEBNotAllowedProcessController.CloseProcess(processes[index2]);
                }
                else
                {
                  this.runningProcessesToClose.Add(processes[index2]);
                  this.runningApplicationsToClose.Add(str1 == "SEB" ? str2 : str1);
                }
              }
            }
          }
        }
      }
      return true;
    }

    public void ShowApplicationChooserForm()
    {
      this.sebApplicationChooserForm.fillListApplications();
      this.sebApplicationChooserForm.Visible = true;
    }

    public void SelectNextListItem()
    {
      this.sebApplicationChooserForm.SelectNextListItem();
    }

    public void HideApplicationChooserForm()
    {
      this.sebApplicationChooserForm.Visible = false;
    }

    public void ShowCloseDialogForm()
    {
      if (!(bool) SEBSettings.settingsCurrent["allowQuit"])
        return;
      SebWindowsClientMain.SEBToForeground();
      if (string.IsNullOrEmpty((string) SEBSettings.settingsCurrent["hashedQuitPassword"]))
      {
        SebWindowsClientForm.SetForegroundWindow(this.Handle);
        this.ShowCloseDialogFormConfirmation();
      }
      else
      {
        SebWindowsClientForm.SetForegroundWindow(this.Handle);
        if ((bool) SEBSettings.settingsCurrent["touchOptimized"])
          this.sebCloseDialogForm.InitializeForTouch();
        else
          this.sebCloseDialogForm.InitializeForNonTouch();
        this.sebCloseDialogForm.Visible = true;
        this.sebCloseDialogForm.Activate();
        this.sebCloseDialogForm.txtQuitPassword.Focus();
      }
    }

    public void ShowCloseDialogFormConfirmation()
    {
      if (this.closeDialogConfirmationIsOpen)
        return;
      this.closeDialogConfirmationIsOpen = true;
      SebWindowsClientMain.SEBToForeground();
      this.TopMost = true;
      if (SEBMessageBox.Show(SEBUIStrings.confirmQuitting, SEBUIStrings.confirmQuittingQuestion, MessageBoxIcon.Question, MessageBoxButtons.OKCancel, false) == DialogResult.OK)
        this.ExitApplication(true);
      else
        this.closeDialogConfirmationIsOpen = false;
    }

    public bool OpenSEBForm()
    {
      Logger.AddInformation("entering Opensebform", (object) null, (Exception) null, (string) null);
      if ((bool) SEBSettings.valueForDictionaryKey(SEBSettings.settingsCurrent, "showTaskBar"))
      {
        Logger.AddInformation("attempting to position the taskbar", (object) null, (Exception) null, (string) null);
        this.SetFormOnDesktop();
        Logger.AddInformation("finished taskbar positioning", (object) null, (Exception) null, (string) null);
      }
      else
      {
        Logger.AddInformation("hiding the taskbar", (object) null, (Exception) null, (string) null);
        this.Visible = false;
        this.Height = 1;
        this.Width = 1;
        this.Location = new Point(-50000, -50000);
        this.taskbarHeight = 0;
        this.PlaceFormOnDesktop(false, true);
      }
      try
      {
        SebWindowsClientMain.CheckIfInsideVirtualMachine();
        SebWindowsClientMain.CheckServicePolicy(SebWindowsServiceHandler.IsServiceAvailable);
        Logger.AddInformation("attempting to start socket server", (object) null, (Exception) null, (string) null);
        SEBXULRunnerWebSocketServer.StartServer();
        try
        {
          Logger.AddInformation("setting registry values", (object) null, (Exception) null, (string) null);
          if (SebWindowsServiceHandler.IsServiceAvailable)
          {
            if (!SebWindowsServiceHandler.SetRegistryAccordingToConfiguration())
            {
              Logger.AddError("Unable to set Registry values", (object) this, (Exception) null, (string) null);
              SebWindowsClientMain.CheckServicePolicy(false);
            }
          }
        }
        catch (SEBNotAllowedToRunEception ex)
        {
          throw;
        }
        catch (Exception ex)
        {
          Logger.AddError("Unable to set Registry values", (object) this, ex, (string) null);
          SebWindowsClientMain.CheckServicePolicy(false);
        }
        try
        {
          Logger.AddInformation("Disabling Windows update", (object) null, (Exception) null, (string) null);
          if (SebWindowsServiceHandler.IsServiceAvailable)
          {
            if (!SebWindowsServiceHandler.DisableWindowsUpdate())
              Logger.AddWarning("Unable to disable Windows update service", (object) this, (Exception) null, (string) null);
          }
        }
        catch (Exception ex)
        {
          Logger.AddError("Unable to disable Windows update service", (object) this, ex, (string) null);
        }
        try
        {
          Logger.AddInformation("killing processes that are not allowed to run", (object) null, (Exception) null, (string) null);
          this.InitClientRegistryAndKillProcesses();
        }
        catch (Exception ex)
        {
          Logger.AddError("Unable to kill processes that are running before start", (object) this, ex, (string) null);
        }
        SebKeyCapture.FilterKeys = true;
        try
        {
          Logger.AddInformation("adding allowed processes to taskbar", (object) null, (Exception) null, (string) null);
          this.addPermittedProcessesToTS();
        }
        catch (Exception ex)
        {
          Logger.AddError("Unable to addPermittedProcessesToTS", (object) this, ex, (string) null);
        }
        if (this.sebCloseDialogForm == null)
        {
          Logger.AddInformation("creating close dialog form", (object) null, (Exception) null, (string) null);
          this.sebCloseDialogForm = new SebCloseDialogForm();
          this.sebCloseDialogForm.TopMost = true;
        }
        if (this.sebApplicationChooserForm == null)
        {
          Logger.AddInformation("building application chooser form", (object) null, (Exception) null, (string) null);
          this.sebApplicationChooserForm = new SebApplicationChooserForm();
          this.sebApplicationChooserForm.TopMost = true;
          this.sebApplicationChooserForm.Show();
          this.sebApplicationChooserForm.Visible = false;
        }
        return true;
      }
      catch (SEBNotAllowedToRunEception ex)
      {
        Logger.AddInformation(string.Format("exiting without starting up because {0}", (object) ex.Message), (object) null, (Exception) null, (string) null);
        this.ExitApplication(false);
        return false;
      }
    }

    public void CloseSEBForm()
    {
      try
      {
        Logger.AddInformation("restoring registry entries", (object) null, (Exception) null, (string) null);
        if (!SebWindowsServiceHandler.IsServiceAvailable)
        {
          Logger.AddInformation("Restarting Service Connection", (object) null, (Exception) null, (string) null);
          SebWindowsServiceHandler.Reconnect();
        }
        Logger.AddInformation("windows service is available", (object) null, (Exception) null, (string) null);
        if (!SebWindowsServiceHandler.ResetRegistry())
          Logger.AddWarning("Unable to reset Registry values", (object) this, (Exception) null, (string) null);
      }
      catch (Exception ex)
      {
        Logger.AddError("Unable to reset Registry values", (object) this, ex, (string) null);
      }
      try
      {
        Logger.AddInformation("attempting to reset workspacearea", (object) null, (Exception) null, (string) null);
        SEBWorkingAreaHandler.ResetWorkspaceArea();
        Logger.AddInformation("workspace area resetted", (object) null, (Exception) null, (string) null);
      }
      catch (Exception ex)
      {
        Logger.AddError("Unable to reset WorkingArea", (object) this, ex, (string) null);
      }
      this.xulRunner.Exited -= new EventHandler(this.xulRunner_Exited);
      Logger.AddInformation("closing processes that where started by seb", (object) null, (Exception) null, (string) null);
      for (int index = 0; index < this.permittedProcessesReferences.Count; ++index)
      {
        try
        {
          Process processToClose = this.permittedProcessesReferences[index];
          if (processToClose != null && !processToClose.HasExited && processToClose.MainWindowHandle == IntPtr.Zero)
            processToClose = SEBWindowHandler.GetWindowHandleByTitle((string) ((Dictionary<string, object>) ((List<object>) SEBClientInfo.getSebSetting("permittedProcesses")["permittedProcesses"])[index])["identifier"]).GetProcess();
          if (processToClose != null)
          {
            if (!processToClose.HasExited)
            {
              Logger.AddInformation("attempting to close " + processToClose.ProcessName, (object) null, (Exception) null, (string) null);
              SEBNotAllowedProcessController.CloseProcess(processToClose);
            }
          }
        }
        catch (Exception ex)
        {
          Logger.AddError("Unable to shutdown process", (object) null, ex, (string) null);
        }
      }
      Logger.AddInformation("clearing running processes list", (object) null, (Exception) null, (string) null);
      this.permittedProcessesReferences.Clear();
      Logger.AddInformation("disabling foreground watchdog", (object) null, (Exception) null, (string) null);
      SEBWindowHandler.DisableForegroundWatchDog();
      Logger.AddInformation("disabling process watchdog", (object) null, (Exception) null, (string) null);
      SEBProcessHandler.DisableProcessWatchDog();
      try
      {
        Logger.AddInformation("restoring hidden windows", (object) null, (Exception) null, (string) null);
        SEBWindowHandler.RestoreHiddenWindows();
      }
      catch (Exception ex)
      {
        Logger.AddError("Unable to restore hidden windows", (object) null, ex, (string) null);
      }
      SEBDesktopWallpaper.Reset();
      if ((bool) SEBClientInfo.getSebSetting("killExplorerShell")["killExplorerShell"])
      {
        if (SEBClientInfo.ExplorerShellWasKilled)
        {
          try
          {
            Logger.AddInformation("Attempting to start Explorer Shell", (object) null, (Exception) null, (string) null);
            SEBProcessHandler.StartExplorerShell(true);
            Logger.AddInformation("Successfully started Explorer Shell", (object) null, (Exception) null, (string) null);
          }
          catch (Exception ex)
          {
            Logger.AddError("Unable to StartExplorerShell", (object) null, ex, (string) null);
          }
        }
      }
      SEBClipboard.CleanClipboard();
      Logger.AddInformation("Clipboard deleted.", (object) null, (Exception) null, (string) null);
      Logger.AddInformation("disabling filtered keys", (object) null, (Exception) null, (string) null);
      SebKeyCapture.FilterKeys = false;
      Logger.AddInformation("returning from closesebform", (object) null, (Exception) null, (string) null);
    }

    public void SebWindowsClientForm_Load(object sender, EventArgs e)
    {
    }

    public void SebWindowsClientForm_FormClosing(object sender, FormClosingEventArgs e)
    {
    }

    public void ExitApplication(bool showLoadingScreen = true)
    {
      if ((bool) SEBSettings.settingsCurrent["createNewDesktop"])
        showLoadingScreen = false;
      Thread thread = (Thread) null;
      if (showLoadingScreen)
      {
        SEBSplashScreen.CloseSplash();
        thread = new Thread(new ThreadStart(SEBLoading.StartLoading));
        thread.Start();
      }
      Logger.AddInformation("Attempting to CloseSEBForm in ExitApplication", (object) null, (Exception) null, (string) null);
      try
      {
        this.CloseSEBForm();
      }
      catch (Exception ex)
      {
        Logger.AddError("Unable to CloseSEBForm()", (object) this, ex, (string) null);
      }
      Logger.AddInformation("Successfull CloseSEBForm", (object) null, (Exception) null, (string) null);
      if (showLoadingScreen)
      {
        Logger.AddInformation("closing loading screen", (object) null, (Exception) null, (string) null);
        SEBLoading.CloseLoading();
        try
        {
          thread.Abort();
        }
        catch
        {
        }
      }
      SebWindowsClientForm.WTSUnRegisterSessionNotification(this.Handle);
      Logger.AddInformation("Attempting to ResetSEBDesktop in ExitApplication", (object) null, (Exception) null, (string) null);
      SebWindowsClientMain.ResetSEBDesktop();
      Logger.AddInformation("Successfull ResetSEBDesktop", (object) null, (Exception) null, (string) null);
      Logger.AddInformation("---------- EXITING SEB - ENDING SESSION -------------", (object) null, (Exception) null, (string) null);
      Application.Exit();
      Environment.Exit(0);
    }

    private void noSelectButton1_Click(object sender, EventArgs e)
    {
      if (!(bool) SEBSettings.settingsCurrent["allowQuit"])
        return;
      this.ShowCloseDialogForm();
    }

    protected override void WndProc(ref Message m)
    {
      if (m.Msg == 689)
      {
        switch ((SebWindowsClientForm.WTSMessage) m.WParam.ToInt32())
        {
          case SebWindowsClientForm.WTSMessage.WTS_CONSOLE_DISCONNECT:
          case SebWindowsClientForm.WTSMessage.WTS_REMOTE_DISCONNECT:
            Logger.AddInformation("Session disconnect detected - closing SEB", (object) null, (Exception) null, (string) null);
            this.ExitApplication(true);
            break;
        }
      }
      base.WndProc(ref m);
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    public void InitializeComponent()
    {
      this.components = (IContainer) new Container();
      ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof (SebWindowsClientForm));
      this.ilProcessIcons = new ImageList(this.components);
      this.taskbarToolStrip = new TaskbarToolStrip();
      this.SuspendLayout();
      this.ilProcessIcons.ImageStream = (ImageListStreamer) componentResourceManager.GetObject("ilProcessIcons.ImageStream");
      this.ilProcessIcons.TransparentColor = System.Drawing.Color.Transparent;
      this.ilProcessIcons.Images.SetKeyName(0, "AcrobatReader");
      this.ilProcessIcons.Images.SetKeyName(1, "calc");
      this.ilProcessIcons.Images.SetKeyName(2, "notepad");
      this.ilProcessIcons.Images.SetKeyName(3, "xulrunner");
      componentResourceManager.ApplyResources((object) this.taskbarToolStrip, "taskbarToolStrip");
      this.taskbarToolStrip.GripStyle = ToolStripGripStyle.Hidden;
      this.taskbarToolStrip.ImageScalingSize = new Size(32, 32);
      this.taskbarToolStrip.Name = "taskbarToolStrip";
      this.taskbarToolStrip.RenderMode = ToolStripRenderMode.System;
      componentResourceManager.ApplyResources((object) this, "$this");
      this.AutoScaleMode = AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.White;
      this.ControlBox = false;
      this.Controls.Add((Control) this.taskbarToolStrip);
      this.ForeColor = SystemColors.ControlLightLight;
      this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = nameof (SebWindowsClientForm);
      this.ShowIcon = false;
      this.TransparencyKey = System.Drawing.Color.Fuchsia;
      this.FormClosing += new FormClosingEventHandler(this.SebWindowsClientForm_FormClosing);
      this.Load += new EventHandler(this.SebWindowsClientForm_Load);
      this.ResumeLayout(false);
      this.PerformLayout();
    }

    public enum WTSMessage
    {
      WTS_CONSOLE_CONNECT = 1,
      WTS_CONSOLE_DISCONNECT = 2,
      WTS_REMOTE_CONNECT = 3,
      WTS_REMOTE_DISCONNECT = 4,
      WTS_SESSION_LOGON = 5,
      WTS_SESSION_LOGOFF = 6,
      WTS_SESSION_LOCK = 7,
      WTS_SESSION_UNLOCK = 8,
      WTS_SESSION_REMOTE_CONTROL = 9,
    }

    private delegate bool EnumThreadProc(IntPtr hwnd, IntPtr lParam);
  }
}
