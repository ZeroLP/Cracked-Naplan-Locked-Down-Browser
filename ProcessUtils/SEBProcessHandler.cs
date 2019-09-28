

using SebWindowsClient.DiagnosticsUtils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace SebWindowsClient.ProcessUtils
{
  public static class SEBProcessHandler
  {
    public static List<string> ProhibitedExecutables = new List<string>();
    private static ProcessWatchDog _processWatchDog;
    private const int WM_USER = 1024;

    public static bool IsProcessProhibited(string processName)
    {
      if (string.IsNullOrWhiteSpace(processName))
        return false;
      processName = processName.ToLower();
      return SEBProcessHandler.ProhibitedExecutables.Count != 0 && SEBProcessHandler.ProhibitedExecutables.Count > 0 && SEBProcessHandler.ProhibitedExecutables.Any<string>((Func<string, bool>) (ex =>
      {
        if (!ex.Contains(processName))
          return processName.Contains(ex);
        return true;
      }));
    }

    public static void StartExplorerShell(bool waitForStartup = true)
    {
      if (!(SEBProcessHandler.FindWindow("Shell_TrayWnd", (string) null) == IntPtr.Zero))
        return;
      string str = string.Format("{0}\\{1}", (object) Environment.GetEnvironmentVariable("WINDIR"), (object) "explorer.exe");
      new Process()
      {
        StartInfo = {
          FileName = str,
          UseShellExecute = true,
          WorkingDirectory = Application.StartupPath,
          CreateNoWindow = true
        }
      }.Start();
      for (int index = 0; index < 6; ++index)
      {
        Logger.AddInformation("waiting for explorer shell to get up " + (object) index + " seconds", (object) null, (Exception) null, (string) null);
        if (!(SEBProcessHandler.FindWindow("Shell_TrayWnd", (string) null) != IntPtr.Zero))
          Thread.Sleep(1000);
        else
          break;
      }
      if (!waitForStartup)
        return;
      Logger.AddInformation("waiting for explorer shell to finish starting 6 seconds", (object) null, (Exception) null, (string) null);
      Thread.Sleep(6000);
    }

    public static bool KillExplorerShell()
    {
      try
      {
        IntPtr window = SEBProcessHandler.FindWindow("Shell_TrayWnd", (string) null);
        if (window != IntPtr.Zero)
        {
          SEBProcessHandler.PostMessage(window, 1460U, IntPtr.Zero, IntPtr.Zero);
          while (SEBProcessHandler.FindWindow("Shell_TrayWnd", (string) null) != IntPtr.Zero)
            Thread.Sleep(500);
        }
        return true;
      }
      catch (Exception ex)
      {
        Logger.AddInformation("{0} {1}", (object) ex.Message, (Exception) null, (string) null);
        return false;
      }
    }

    public static void EnableProcessWatchDog()
    {
      if (SEBProcessHandler._processWatchDog == null)
        SEBProcessHandler._processWatchDog = new ProcessWatchDog();
      SEBProcessHandler._processWatchDog.StartWatchDog();
    }

    public static void DisableProcessWatchDog()
    {
      if (SEBProcessHandler._processWatchDog == null)
        return;
      SEBProcessHandler._processWatchDog.StopWatchDog();
      SEBProcessHandler._processWatchDog = (ProcessWatchDog) null;
    }

    public static string GetExecutableName(this Process process)
    {
      try
      {
        return process.ProcessName;
      }
      catch (Exception ex)
      {
        Logger.AddWarning("Unable to GetExecutableName of process", (object) null, (Exception) null, (string) null);
        return "";
      }
    }

    public static IEnumerable<KeyValuePair<IntPtr, string>> GetOpenWindows(this Process process)
    {
      return SEBWindowHandler.GetOpenWindows().Where<KeyValuePair<IntPtr, string>>((Func<KeyValuePair<IntPtr, string>, bool>) (oW => oW.Key.GetProcess().GetExecutableName() == process.GetExecutableName()));
    }

    public static void PreventSleep()
    {
      if (SEBProcessHandler.SetThreadExecutionState(SEBProcessHandler.EXECUTION_STATE.ES_SYSTEM_REQUIRED | SEBProcessHandler.EXECUTION_STATE.ES_DISPLAY_REQUIRED | SEBProcessHandler.EXECUTION_STATE.ES_AWAYMODE_REQUIRED | SEBProcessHandler.EXECUTION_STATE.ES_CONTINUOUS) != (SEBProcessHandler.EXECUTION_STATE) 0)
        return;
      int num = (int) SEBProcessHandler.SetThreadExecutionState(SEBProcessHandler.EXECUTION_STATE.ES_SYSTEM_REQUIRED | SEBProcessHandler.EXECUTION_STATE.ES_DISPLAY_REQUIRED | SEBProcessHandler.EXECUTION_STATE.ES_CONTINUOUS);
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool PostMessage(IntPtr hWnd, [MarshalAs(UnmanagedType.U4)] uint Msg, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern SEBProcessHandler.EXECUTION_STATE SetThreadExecutionState(SEBProcessHandler.EXECUTION_STATE esFlags);

    [Flags]
    public enum EXECUTION_STATE : uint
    {
      ES_SYSTEM_REQUIRED = 1,
      ES_DISPLAY_REQUIRED = 2,
      ES_AWAYMODE_REQUIRED = 64,
      ES_CONTINUOUS = 2147483648,
    }
  }
}
