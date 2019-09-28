

using Microsoft.Win32;
using SebWindowsClient.ConfigurationUtils;
using SebWindowsClient.ProcessUtils;
using SebWindowsClient.XULRunnerCommunication;
using System;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using System.Timers;

namespace SebWindowsClient.UI
{
  public static class TapTipHandler
  {
    public static bool FirstOpen = true;
    private static bool _textFocusHappened;
    public const uint WS_DISABLED = 134217728;
    public const int GWL_STYLE = -16;

    public static event TapTipHandler.KeyboardStateChangedEventHandler OnKeyboardStateChanged;

    public static void RegisterXulRunnerEvents()
    {
      if ((int) SEBSettings.settingsCurrent["oskBehavior"] == 1)
        return;
      SEBXULRunnerWebSocketServer.OnXulRunnerTextFocus += new EventHandler(TapTipHandler.OnXulRunnerTextFocus);
      SEBXULRunnerWebSocketServer.OnXulRunnerTextBlur += new EventHandler(TapTipHandler.OnXulRunnerTextBlur);
    }

    private static void OnXulRunnerTextFocus(object sender, EventArgs e)
    {
      TapTipHandler._textFocusHappened = true;
      TapTipHandler.ShowKeyboard(false);
    }

    private static void OnXulRunnerTextBlur(object sender, EventArgs e)
    {
      TapTipHandler._textFocusHappened = false;
      Timer t = new Timer() { Interval = 100.0 };
      t.Elapsed += (ElapsedEventHandler) ((x, y) =>
      {
        if (!TapTipHandler._textFocusHappened)
          TapTipHandler.HideKeyboard();
        t.Stop();
      });
      t.Start();
    }

    public static void ShowKeyboard(bool force = false)
    {
      try
      {
        if (TapTipHandler.IsPhysicalKeyboardAttached() && !force && (int) SEBSettings.settingsCurrent["oskBehavior"] == 2 || (!(bool) SEBSettings.valueForDictionaryKey(SEBSettings.settingsCurrent, "touchOptimized") || TapTipHandler.IsKeyboardVisible()))
          return;
        if (!SEBWindowHandler.AllowedExecutables.Contains("tabtip.exe"))
          SEBWindowHandler.AllowedExecutables.Add("tabtip.exe");
        Process.Start(Path.Combine("C:\\Program Files", "Common Files\\Microsoft Shared\\ink", "TabTip.exe"));
        // ISSUE: reference to a compiler-generated field
        if (TapTipHandler.OnKeyboardStateChanged == null)
          return;
        Timer t = new Timer() { Interval = 500.0 };
        t.Elapsed += (ElapsedEventHandler) ((sender, args) =>
        {
          if (TapTipHandler.IsKeyboardVisible())
            return;
          t.Stop();
          // ISSUE: reference to a compiler-generated field
          TapTipHandler.OnKeyboardStateChanged(false);
        });
        t.Start();
        // ISSUE: reference to a compiler-generated field
        TapTipHandler.OnKeyboardStateChanged(true);
      }
      catch
      {
      }
    }

    public static void HideKeyboard()
    {
      if (!TapTipHandler.IsKeyboardVisible())
        return;
      TapTipHandler.PostMessage(TapTipHandler.GetKeyboardWindowHandle(), 274U, new IntPtr(61536), (IntPtr) 0);
    }

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    public static extern IntPtr FindWindow(string sClassName, string sAppName);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint GetWindowLong(IntPtr hWnd, int nIndex);

    public static IntPtr GetKeyboardWindowHandle()
    {
      return TapTipHandler.FindWindow("IPTip_Main_Window", (string) null);
    }

    public static bool IsKeyboardVisible()
    {
      if (TapTipHandler.FirstOpen)
      {
        TapTipHandler.FirstOpen = false;
        return false;
      }
      IntPtr keyboardWindowHandle = TapTipHandler.GetKeyboardWindowHandle();
      bool flag = false;
      if (keyboardWindowHandle != IntPtr.Zero)
      {
        keyboardWindowHandle.MaximizeWindow();
        flag = ((int) TapTipHandler.GetWindowLong(keyboardWindowHandle, -16) & 134217728) != 134217728;
      }
      return flag;
    }

    public static bool IsPhysicalKeyboardAttached()
    {
      ManagementObjectCollection objectCollection = new ManagementObjectSearcher("Select Description from Win32_Keyboard").Get();
      if (objectCollection.Count == 1)
      {
        foreach (ManagementBaseObject managementBaseObject in objectCollection)
        {
          if (managementBaseObject.GetPropertyValue("Description").ToString().Contains("HID"))
            return false;
        }
      }
      return true;
    }

    public static bool IsKeyboardDocked()
    {
      int num = 1;
      try
      {
        num = (int) Registry.GetValue("HKEY_CURRENT_USER\\Software\\Microsoft\\TabletTip\\1.7\\", "EdgeTargetDockedState", (object) 1);
      }
      catch
      {
      }
      return num == 1;
    }

    public delegate void KeyboardStateChangedEventHandler(bool shown);
  }
}
