

using SebWindowsClient.DiagnosticsUtils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace SebWindowsClient.ProcessUtils
{
  public static class SEBWindowHandler
  {
    public static List<string> AllowedExecutables = new List<string>();
    public static ForegroundWatchDog ForegroundWatchDog = new ForegroundWatchDog();
    private static List<IntPtr> _hiddenWindowHandles = new List<IntPtr>();
    private const int WM_COMMAND = 273;
    private const int MIN_ALL = 419;
    private const int WM_CLOSE = 16;
    private const int SC_MINIMIZE = 61472;
    private const int SC_MAXIMIZE = 61488;
    private const int SC_CLOSE = 61536;

    public static bool IsWindowAllowedByProcessName(string processName)
    {
      if (string.IsNullOrWhiteSpace(processName))
        return false;
      processName = processName.ToLower();
      return SEBWindowHandler.AllowedExecutables.Count == 0 || SEBWindowHandler.AllowedExecutables.Count > 0 && SEBWindowHandler.AllowedExecutables.Any<string>((Func<string, bool>) (ex =>
      {
        if (!ex.Contains(processName))
          return processName.Contains(ex);
        return true;
      }));
    }

    public static IDictionary<IntPtr, string> GetOpenWindows()
    {
      try
      {
        IntPtr lShellWindow = SEBWindowHandler.GetShellWindow();
        Dictionary<IntPtr, string> lWindows = new Dictionary<IntPtr, string>();
        SEBWindowHandler.EnumWindows((SEBWindowHandler.EnumWindowsProc) ((hWnd, lParam) =>
        {
          if (hWnd == lShellWindow || !SEBWindowHandler.IsWindowVisible(hWnd))
            return true;
          int windowTextLength = SEBWindowHandler.GetWindowTextLength(hWnd);
          if (windowTextLength == 0)
            return true;
          StringBuilder text = new StringBuilder(windowTextLength);
          SEBWindowHandler.GetWindowText(hWnd, text, windowTextLength + 1);
          lWindows[hWnd] = text.ToString().ToLower();
          return true;
        }), 0);
        return (IDictionary<IntPtr, string>) lWindows;
      }
      catch (Exception ex)
      {
        Logger.AddError("Unable to retrieve open windows", (object) null, ex, (string) null);
        return (IDictionary<IntPtr, string>) new Dictionary<IntPtr, string>();
      }
    }

    public static void HideAllOpenWindows()
    {
      SEBWindowHandler.EditAllOpenWindows(SEBWindowHandler.ShowWindowCommand.SW_HIDE);
    }

    public static void MinimizeAllOpenWindows(bool force = false)
    {
      if (force)
        SEBWindowHandler.SendMessage(SEBWindowHandler.FindWindow("Shell_TrayWnd", (string) null), 273, (IntPtr) 419, IntPtr.Zero);
      else
        SEBWindowHandler.EditAllOpenWindows(SEBWindowHandler.ShowWindowCommand.SW_SHOWMINIMIZED);
    }

    public static List<IntPtr> GetWindowHandlesByTitle(string title)
    {
      try
      {
        title = title.ToLower();
        return SEBWindowHandler.GetOpenWindows().Where<KeyValuePair<IntPtr, string>>((Func<KeyValuePair<IntPtr, string>, bool>) (lWindow => lWindow.Value.Contains(title))).Select<KeyValuePair<IntPtr, string>, IntPtr>((Func<KeyValuePair<IntPtr, string>, IntPtr>) (lWindow => lWindow.Key)).ToList<IntPtr>();
      }
      catch (Exception ex)
      {
        Logger.AddError("Unable to GetWindowHandlesByTitle " + title, (object) null, ex, (string) null);
        return new List<IntPtr>();
      }
    }

    public static IntPtr GetWindowHandleByTitle(string title)
    {
      try
      {
        title = title.ToLower();
        return SEBWindowHandler.GetOpenWindows().FirstOrDefault<KeyValuePair<IntPtr, string>>((Func<KeyValuePair<IntPtr, string>, bool>) (lWindow => lWindow.Value.Contains(title))).Key;
      }
      catch (Exception ex)
      {
        return IntPtr.Zero;
      }
    }

    public static void RestoreHiddenWindows()
    {
      foreach (IntPtr hiddenWindowHandle in SEBWindowHandler._hiddenWindowHandles)
        SEBWindowHandler.EditWindowByHandle(hiddenWindowHandle, SEBWindowHandler.ShowWindowCommand.SW_RESTORE, false);
    }

    public static void HideWindow(this IntPtr handle)
    {
      SEBWindowHandler.EditWindowByHandle(handle, SEBWindowHandler.ShowWindowCommand.SW_HIDE, false);
    }

    public static void MinimizeWindow(this IntPtr windowHandle, bool waitForShowingUp = false)
    {
      SEBWindowHandler.EditWindowByHandle(windowHandle, SEBWindowHandler.ShowWindowCommand.SW_SHOWMINIMIZED, waitForShowingUp);
    }

    public static void MaximizeWindow(this IntPtr windowHandle)
    {
      SEBWindowHandler.EditWindowByHandle(windowHandle, SEBWindowHandler.ShowWindowCommand.SW_SHOWMAXIMIZED, false);
    }

    public static void AdaptWindowToWorkingArea(this IntPtr windowHandle, int? taskbarHeight = null)
    {
      SEBWindowHandler.EditWindowByHandle(windowHandle, SEBWindowHandler.ShowWindowCommand.SW_SHOWNORMAL, false);
      if (taskbarHeight.HasValue)
      {
        IntPtr hWnd = windowHandle;
        int X = 0;
        int Y = 0;
        Rectangle bounds = Screen.PrimaryScreen.Bounds;
        int width = bounds.Width;
        bounds = Screen.PrimaryScreen.Bounds;
        int nHeight = bounds.Height - taskbarHeight.Value;
        int num = 1;
        SEBWindowHandler.MoveWindow(hWnd, X, Y, width, nHeight, num != 0);
      }
      else
      {
        IntPtr hWnd = windowHandle;
        int X = 0;
        int Y = 0;
        Rectangle rectangle = Screen.PrimaryScreen.Bounds;
        int width = rectangle.Width;
        rectangle = Screen.PrimaryScreen.WorkingArea;
        int height = rectangle.Height;
        int num = 1;
        SEBWindowHandler.MoveWindow(hWnd, X, Y, width, height, num != 0);
      }
      SEBWindowHandler.EditWindowByHandle(windowHandle, SEBWindowHandler.ShowWindowCommand.SW_SHOWMAXIMIZED, false);
    }

    public static int GetWindowHeight(this IntPtr windowHandle)
    {
      SEBWindowHandler.RECT lpRect = new SEBWindowHandler.RECT();
      if (!SEBWindowHandler.GetWindowRect(new HandleRef((object) null, windowHandle), out lpRect))
        return 0;
      return lpRect.Bottom - lpRect.Top;
    }

    public static string GetWindowTitle(this IntPtr windowHandle)
    {
      try
      {
        StringBuilder text = new StringBuilder(256);
        string str = "";
        if (SEBWindowHandler.GetWindowText(windowHandle, text, 256) > 0)
          str = text.ToString();
        return str;
      }
      catch (Exception ex)
      {
        Logger.AddError("Unable to GetWindowTitle", (object) null, ex, (string) null);
        return "";
      }
    }

    public static Process GetProcess(this IntPtr windowHandle)
    {
      try
      {
        uint lpdwProcessId;
        int windowThreadProcessId = (int) SEBWindowHandler.GetWindowThreadProcessId(windowHandle, out lpdwProcessId);
        return Process.GetProcessById((int) lpdwProcessId);
      }
      catch (Exception ex)
      {
        Logger.AddError("Unable to get Process", (object) null, ex, (string) null);
        return (Process) null;
      }
    }

    public static void BringToTop(this IntPtr windowHandle, bool restoreIfMinimizedOrHidden = true)
    {
      SEBWindowHandler.SetForegroundWindow(windowHandle);
      SEBWindowHandler.BringWindowToTop(windowHandle);
      int showCmd = windowHandle.GetPlacement().showCmd;
      if (!restoreIfMinimizedOrHidden || showCmd != 2 && showCmd != 0)
        return;
      SEBWindowHandler.EditWindowByHandle(windowHandle, SEBWindowHandler.ShowWindowCommand.SW_RESTORE, false);
    }

    public static void CloseWindow(this IntPtr windowHandle)
    {
      SEBWindowHandler.SendMessage(windowHandle, 16, IntPtr.Zero, IntPtr.Zero);
    }

    private static SEBWindowHandler.WINDOWPLACEMENT GetPlacement(this IntPtr hwnd)
    {
      SEBWindowHandler.WINDOWPLACEMENT lpwndpl = new SEBWindowHandler.WINDOWPLACEMENT();
      lpwndpl.length = Marshal.SizeOf((object) lpwndpl);
      SEBWindowHandler.GetWindowPlacement(hwnd, ref lpwndpl);
      return lpwndpl;
    }

    public static bool IsAllowed(this IntPtr windowHandle)
    {
      Process process = windowHandle.GetProcess();
      if (process == null)
        return false;
      return process.IsWindowAllowed();
    }

    public static void EnableForegroundWatchDog()
    {
      SEBWindowHandler.ForegroundWatchDog.StartWatchDog();
    }

    public static void DisableForegroundWatchDog()
    {
      if (SEBWindowHandler.ForegroundWatchDog == null)
        return;
      SEBWindowHandler.ForegroundWatchDog.StopWatchDog();
    }

    public static bool IsWindowAllowed(this Process process)
    {
      return SEBWindowHandler.IsWindowAllowedByProcessName(process.GetExecutableName());
    }

    private static void EditAllOpenWindows(SEBWindowHandler.ShowWindowCommand action)
    {
      foreach (KeyValuePair<IntPtr, string> openWindow in (IEnumerable<KeyValuePair<IntPtr, string>>) SEBWindowHandler.GetOpenWindows())
      {
        IntPtr key = openWindow.Key;
        if (!key.IsAllowed())
          SEBWindowHandler.EditWindowByHandle(key, action, false);
      }
    }

    private static void EditWindowByHandle(IntPtr windowHandle, SEBWindowHandler.ShowWindowCommand action, bool waitForShowingUp = false)
    {
      if (action == SEBWindowHandler.ShowWindowCommand.SW_HIDE)
        SEBWindowHandler._hiddenWindowHandles.Add(windowHandle);
      ThreadPool.QueueUserWorkItem((WaitCallback) (_param1 =>
      {
        try
        {
          if (action == SEBWindowHandler.ShowWindowCommand.SW_SHOWMINIMIZED & waitForShowingUp)
            Thread.Sleep(500);
          if (!SEBWindowHandler.ShowWindowAsync(windowHandle, (int) action))
          {
            if (action == SEBWindowHandler.ShowWindowCommand.SW_SHOWMINIMIZED)
            {
              SEBWindowHandler.SendMessage(windowHandle, 274, (IntPtr) 61472, IntPtr.Zero);
              Logger.AddInformation(string.Format("Window {0} minimized", (object) windowHandle.GetWindowTitle()), (object) null, (Exception) null, (string) null);
            }
            else if (action == SEBWindowHandler.ShowWindowCommand.SW_HIDE)
            {
              SEBWindowHandler.SendMessage(windowHandle, 274, (IntPtr) 61536, IntPtr.Zero);
              Logger.AddInformation(string.Format("Window {0} closed, because i was unable to hide it", (object) windowHandle.GetWindowTitle()), (object) null, (Exception) null, (string) null);
            }
            else
            {
              if (action != SEBWindowHandler.ShowWindowCommand.SW_SHOWMAXIMIZED)
                return;
              SEBWindowHandler.SendMessage(windowHandle, 274, (IntPtr) 61488, IntPtr.Zero);
              Logger.AddInformation(string.Format("Window {0} Maximized", (object) windowHandle.GetWindowTitle()), (object) null, (Exception) null, (string) null);
            }
          }
          else
            Logger.AddInformation(string.Format("Window {0} {1}", (object) windowHandle.GetWindowTitle(), (object) action.ToString()), (object) null, (Exception) null, (string) null);
        }
        catch
        {
          try
          {
            if (action == SEBWindowHandler.ShowWindowCommand.SW_SHOWMINIMIZED)
            {
              SEBWindowHandler.SendMessage(windowHandle, 274, (IntPtr) 61472, IntPtr.Zero);
              Logger.AddInformation(string.Format("Window {0} minimized", (object) windowHandle.GetWindowTitle()), (object) null, (Exception) null, (string) null);
            }
            else if (action == SEBWindowHandler.ShowWindowCommand.SW_HIDE)
            {
              SEBWindowHandler.SendMessage(windowHandle, 274, (IntPtr) 61536, IntPtr.Zero);
              Logger.AddInformation(string.Format("Window {0} closed, because i was unable to hide it", (object) windowHandle.GetWindowTitle()), (object) null, (Exception) null, (string) null);
            }
            else
            {
              if (action != SEBWindowHandler.ShowWindowCommand.SW_SHOWMAXIMIZED)
                return;
              SEBWindowHandler.SendMessage(windowHandle, 274, (IntPtr) 61488, IntPtr.Zero);
              Logger.AddInformation(string.Format("Window {0} Maximized", (object) windowHandle.GetWindowTitle()), (object) null, (Exception) null, (string) null);
            }
          }
          catch (Exception ex)
          {
          }
        }
      }));
    }

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

    [DllImport("user32.dll")]
    private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

    [DllImport("user32.dll")]
    public static extern uint GetWindowThreadProcessId(IntPtr hwnd, out uint lpdwProcessId);

    [DllImport("USER32.DLL")]
    private static extern bool EnumWindows(SEBWindowHandler.EnumWindowsProc enumFunc, int lParam);

    [DllImport("USER32.DLL")]
    private static extern int GetWindowTextLength(IntPtr hWnd);

    [DllImport("USER32.DLL")]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("USER32.DLL")]
    private static extern IntPtr GetShellWindow();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("User32.dll")]
    private static extern IntPtr SetForegroundWindow(IntPtr hWnd);

    [DllImport("User32.dll")]
    public static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    public static extern bool BringWindowToTop([In] IntPtr hWnd);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetWindowPlacement(IntPtr hWnd, ref SEBWindowHandler.WINDOWPLACEMENT lpwndpl);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetWindowRect(HandleRef hWnd, out SEBWindowHandler.RECT lpRect);

    private enum ShowWindowCommand
    {
      SW_HIDE = 0,
      SW_SHOWNORMAL = 1,
      SW_SHOWMINIMIZED = 2,
      SW_SHOWMAXIMIZED = 3,
      SW_SHOWNOACTIVATE = 4,
      SW_RESTORE = 9,
      SW_SHOWDEFAULT = 10,
      SW_MAX = 11,
    }

    private delegate bool EnumWindowsProc(IntPtr hWnd, int lParam);

    private struct WINDOWPLACEMENT
    {
      public int length;
      public int flags;
      public int showCmd;
      public Point ptMinPosition;
      public Point ptMaxPosition;
      public Rectangle rcNormalPosition;
    }

    public struct RECT
    {
      public int Left;
      public int Top;
      public int Right;
      public int Bottom;
    }
  }
}
