
using SebWindowsClient.ConfigurationUtils;
using SebWindowsClient.DiagnosticsUtils;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SebWindowsClient.BlockShortcutsUtils
{
  public class SebKeyCapture
  {
    private static bool _FilterKeys = true;
    private static bool exitKey1_Pressed = false;
    private static bool exitKey2_Pressed = false;
    private static bool exitKey3_Pressed = false;
    private static bool ctrl_Pressed = false;
    private static bool Q_Pressed = false;
    private static bool Alt_Pressed = false;
    private static bool Tab_Pressed = false;
    private static bool Tab_Pressed_First_Time = true;
    private const int WH_MOUSE_LL = 14;
    private const int WM_KEYDOWN = 256;
    private const int WM_KEYUP = 257;
    private const int WM_SYSKEYDOWN = 260;
    private const int WM_SYSKEYUP = 261;
    private const int WM_SYSCHAR = 262;
    private const int WH_KEYBOARD_LL = 13;
    private static IntPtr ptrKeyboardHook;
    private static IntPtr ptrMouseHook;
    private static SebKeyCapture.LowLevelProc objKeyboardProcess;
    private static SebKeyCapture.LowLevelProc objMouseProcess;
    private static Keys exitKey1;
    private static Keys exitKey2;
    private static Keys exitKey3;
    private static DateTime TouchExitSequenceStartedTime;
    private static int TouchExitSequenceStartedX;

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int id, SebKeyCapture.LowLevelProc callback, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool UnhookWindowsHookEx(IntPtr hook);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hook, int nCode, IntPtr wp, IntPtr lp);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string name);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern short GetAsyncKeyState(Keys key);

    public static bool FilterKeys
    {
      get
      {
        return SebKeyCapture._FilterKeys;
      }
      set
      {
        SebKeyCapture._FilterKeys = value;
        if (value)
        {
          SebKeyCapture.UnregisterKeyboardHookMethod();
          SebKeyCapture.RegisterKeyboardHookMethod();
        }
        else
          SebKeyCapture.UnregisterKeyboardHookMethod();
      }
    }

    private static bool DisableMouseButton(int nCode, IntPtr wp, IntPtr lp)
    {
      SebKeyCapture.MSLLHOOKSTRUCT structure = (SebKeyCapture.MSLLHOOKSTRUCT) Marshal.PtrToStructure(lp, typeof (SebKeyCapture.MSLLHOOKSTRUCT));
      return !(bool) SEBClientInfo.getSebSetting("enableRightMouse")["enableRightMouse"] && nCode >= 0 && (516 == (int) wp || 517 == (int) wp) || !(bool) SEBClientInfo.getSebSetting("enableAltMouseWheel")["enableAltMouseWheel"] && ((Control.ModifierKeys & Keys.Alt) != Keys.None && ((SebKeyCapture.KBDLLHOOKSTRUCT) Marshal.PtrToStructure(lp, typeof (SebKeyCapture.KBDLLHOOKSTRUCT))).flags < 0);
    }

    private static bool DisableKey(IntPtr wp, IntPtr lp)
    {
      SebKeyCapture.KBDLLHOOKSTRUCT structure = (SebKeyCapture.KBDLLHOOKSTRUCT) Marshal.PtrToStructure(lp, typeof (SebKeyCapture.KBDLLHOOKSTRUCT));
      try
      {
        if (!(bool) SEBClientInfo.getSebSetting("enableEsc")["enableEsc"] && structure.key == Keys.Escape || !(bool) SEBClientInfo.getSebSetting("enableCtrlEsc")["enableCtrlEsc"] && structure.flags == 0 && structure.key == Keys.Escape || (!(bool) SEBClientInfo.getSebSetting("enableAltEsc")["enableAltEsc"] && structure.flags == 32 && structure.key == Keys.Escape || !(bool) SEBClientInfo.getSebSetting("enableAltTab")["enableAltTab"] && structure.flags == 32 && structure.key == Keys.Tab) || (!(bool) SEBClientInfo.getSebSetting("enablePrintScreen")["enablePrintScreen"] && structure.key == Keys.Snapshot || !(bool) SEBClientInfo.getSebSetting("enableRightMouse")["enableRightMouse"] && structure.key == Keys.Apps))
          return true;
        int num = (bool) SEBClientInfo.getSebSetting("enableAltTab")["enableAltTab"] ? 1 : 0;
        if (!(bool) SEBClientInfo.getSebSetting("enableAltF4")["enableAltF4"] && structure.flags == 32 && structure.key == Keys.F4 || !(bool) SEBClientInfo.getSebSetting("enableF1")["enableF1"] && structure.key == Keys.F1 || (!(bool) SEBClientInfo.getSebSetting("enableF2")["enableF2"] && structure.key == Keys.F2 || !(bool) SEBClientInfo.getSebSetting("enableF3")["enableF3"] && structure.key == Keys.F3) || (!(bool) SEBClientInfo.getSebSetting("enableF4")["enableF4"] && structure.key == Keys.F4 || !(bool) SEBClientInfo.getSebSetting("enableF5")["enableF5"] && structure.key == Keys.F5 || (!(bool) SEBClientInfo.getSebSetting("enableF6")["enableF6"] && structure.key == Keys.F6 || !(bool) SEBClientInfo.getSebSetting("enableF7")["enableF7"] && structure.key == Keys.F7)) || (!(bool) SEBClientInfo.getSebSetting("enableF8")["enableF8"] && structure.key == Keys.F8 || !(bool) SEBClientInfo.getSebSetting("enableF9")["enableF9"] && structure.key == Keys.F9 || (!(bool) SEBClientInfo.getSebSetting("enableF10")["enableF10"] && structure.key == Keys.F10 || !(bool) SEBClientInfo.getSebSetting("enableF11")["enableF11"] && structure.key == Keys.F11)))
          return true;
        if (!(bool) SEBClientInfo.getSebSetting("enableF12")["enableF12"])
        {
          if (structure.key == Keys.F12)
            return true;
        }
      }
      catch (Exception ex)
      {
        Logger.AddError("DisableKey: Failed with error. " + ex.Message, (object) null, ex, (string) null);
      }
      return false;
    }

    private static void SetExitKeys()
    {
      int num1 = (int) SEBClientInfo.getSebSetting("exitKey1")["exitKey1"];
      int num2 = (int) SEBClientInfo.getSebSetting("exitKey2")["exitKey2"];
      int num3 = (int) SEBClientInfo.getSebSetting("exitKey3")["exitKey3"];
      switch (num1)
      {
        case 0:
          SebKeyCapture.exitKey1 = Keys.F1;
          break;
        case 1:
          SebKeyCapture.exitKey1 = Keys.F2;
          break;
        case 2:
          SebKeyCapture.exitKey1 = Keys.F3;
          break;
        case 3:
          SebKeyCapture.exitKey1 = Keys.F4;
          break;
        case 4:
          SebKeyCapture.exitKey1 = Keys.F5;
          break;
        case 5:
          SebKeyCapture.exitKey1 = Keys.F6;
          break;
        case 6:
          SebKeyCapture.exitKey1 = Keys.F7;
          break;
        case 7:
          SebKeyCapture.exitKey1 = Keys.F8;
          break;
        case 8:
          SebKeyCapture.exitKey1 = Keys.F9;
          break;
        case 9:
          SebKeyCapture.exitKey1 = Keys.F10;
          break;
        case 10:
          SebKeyCapture.exitKey1 = Keys.F11;
          break;
        case 11:
          SebKeyCapture.exitKey1 = Keys.F12;
          break;
        default:
          SebKeyCapture.exitKey1 = Keys.F3;
          break;
      }
      switch (num2)
      {
        case 0:
          SebKeyCapture.exitKey2 = Keys.F1;
          break;
        case 1:
          SebKeyCapture.exitKey2 = Keys.F2;
          break;
        case 2:
          SebKeyCapture.exitKey2 = Keys.F3;
          break;
        case 3:
          SebKeyCapture.exitKey2 = Keys.F4;
          break;
        case 4:
          SebKeyCapture.exitKey2 = Keys.F5;
          break;
        case 5:
          SebKeyCapture.exitKey2 = Keys.F6;
          break;
        case 6:
          SebKeyCapture.exitKey2 = Keys.F7;
          break;
        case 7:
          SebKeyCapture.exitKey2 = Keys.F8;
          break;
        case 8:
          SebKeyCapture.exitKey2 = Keys.F9;
          break;
        case 9:
          SebKeyCapture.exitKey2 = Keys.F10;
          break;
        case 10:
          SebKeyCapture.exitKey2 = Keys.F11;
          break;
        case 11:
          SebKeyCapture.exitKey2 = Keys.F12;
          break;
        default:
          SebKeyCapture.exitKey2 = Keys.F11;
          break;
      }
      switch (num3)
      {
        case 0:
          SebKeyCapture.exitKey3 = Keys.F1;
          break;
        case 1:
          SebKeyCapture.exitKey3 = Keys.F2;
          break;
        case 2:
          SebKeyCapture.exitKey3 = Keys.F3;
          break;
        case 3:
          SebKeyCapture.exitKey3 = Keys.F4;
          break;
        case 4:
          SebKeyCapture.exitKey3 = Keys.F5;
          break;
        case 5:
          SebKeyCapture.exitKey3 = Keys.F6;
          break;
        case 6:
          SebKeyCapture.exitKey3 = Keys.F7;
          break;
        case 7:
          SebKeyCapture.exitKey3 = Keys.F8;
          break;
        case 8:
          SebKeyCapture.exitKey3 = Keys.F9;
          break;
        case 9:
          SebKeyCapture.exitKey3 = Keys.F10;
          break;
        case 10:
          SebKeyCapture.exitKey3 = Keys.F11;
          break;
        case 11:
          SebKeyCapture.exitKey3 = Keys.F12;
          break;
        default:
          SebKeyCapture.exitKey3 = Keys.F6;
          break;
      }
    }

    private static bool SetAndTestCtrlQExitSequence(IntPtr wp, IntPtr lp)
    {
      SebKeyCapture.KBDLLHOOKSTRUCT structure = (SebKeyCapture.KBDLLHOOKSTRUCT) Marshal.PtrToStructure(lp, typeof (SebKeyCapture.KBDLLHOOKSTRUCT));
      if (structure.key == Keys.LControlKey)
        SebKeyCapture.ctrl_Pressed = true;
      else if (structure.key == Keys.RControlKey)
        SebKeyCapture.ctrl_Pressed = true;
      else if (structure.key == Keys.Q)
      {
        SebKeyCapture.Q_Pressed = true;
      }
      else
      {
        SebKeyCapture.ctrl_Pressed = false;
        SebKeyCapture.Q_Pressed = false;
      }
      return SebKeyCapture.ctrl_Pressed && SebKeyCapture.Q_Pressed;
    }

    private static void ResetCtrlQExitSequence(IntPtr wp, IntPtr lp)
    {
      SebKeyCapture.KBDLLHOOKSTRUCT structure = (SebKeyCapture.KBDLLHOOKSTRUCT) Marshal.PtrToStructure(lp, typeof (SebKeyCapture.KBDLLHOOKSTRUCT));
      if (structure.key == Keys.LControlKey)
        SebKeyCapture.ctrl_Pressed = false;
      if (structure.key == Keys.RControlKey)
        SebKeyCapture.ctrl_Pressed = false;
      if (structure.key != Keys.Q)
        return;
      SebKeyCapture.Q_Pressed = false;
    }

    private static bool SetAndTestExitKeySequence(IntPtr wp, IntPtr lp)
    {
      SebKeyCapture.KBDLLHOOKSTRUCT structure = (SebKeyCapture.KBDLLHOOKSTRUCT) Marshal.PtrToStructure(lp, typeof (SebKeyCapture.KBDLLHOOKSTRUCT));
      SebKeyCapture.SetExitKeys();
      if (structure.key == SebKeyCapture.exitKey1)
        SebKeyCapture.exitKey1_Pressed = true;
      else if (structure.key == SebKeyCapture.exitKey2)
        SebKeyCapture.exitKey2_Pressed = true;
      else if (structure.key == SebKeyCapture.exitKey3)
      {
        SebKeyCapture.exitKey3_Pressed = true;
      }
      else
      {
        SebKeyCapture.exitKey1_Pressed = false;
        SebKeyCapture.exitKey2_Pressed = false;
        SebKeyCapture.exitKey3_Pressed = false;
      }
      return SebKeyCapture.exitKey1_Pressed && SebKeyCapture.exitKey2_Pressed && SebKeyCapture.exitKey3_Pressed;
    }

    private static void ResetExitKeySequence(IntPtr wp, IntPtr lp)
    {
      SebKeyCapture.KBDLLHOOKSTRUCT structure = (SebKeyCapture.KBDLLHOOKSTRUCT) Marshal.PtrToStructure(lp, typeof (SebKeyCapture.KBDLLHOOKSTRUCT));
      if (structure.key == SebKeyCapture.exitKey1)
        SebKeyCapture.exitKey1_Pressed = false;
      if (structure.key == SebKeyCapture.exitKey2)
        SebKeyCapture.exitKey2_Pressed = false;
      if (structure.key != SebKeyCapture.exitKey3)
        return;
      SebKeyCapture.exitKey3_Pressed = false;
    }

    private static IntPtr CaptureMouseButton(int nCode, IntPtr wp, IntPtr lp)
    {
      if (nCode >= 0)
      {
        if ((bool) SEBSettings.valueForDictionaryKey(SEBSettings.settingsCurrent, "touchOptimized") && (bool) SEBSettings.valueForDictionaryKey(SEBSettings.settingsCurrent, "enableTouchExit"))
          SebKeyCapture.TestTouchExitSequence(Cursor.Position);
        if (SebKeyCapture.DisableMouseButton(nCode, wp, lp))
          return (IntPtr) 1;
      }
      return SebKeyCapture.CallNextHookEx(SebKeyCapture.ptrMouseHook, nCode, wp, lp);
    }

    private static void TestTouchExitSequence(Point cursorsPosition)
    {
      if (cursorsPosition.Y == 0)
      {
        SebKeyCapture.TouchExitSequenceStartedTime = DateTime.Now;
        SebKeyCapture.TouchExitSequenceStartedX = cursorsPosition.X;
      }
      else
      {
        int num1 = Math.Abs(cursorsPosition.X - SebKeyCapture.TouchExitSequenceStartedX);
        Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;
        int num2 = workingArea.Width / 3;
        if (num1 >= num2 || !(DateTime.Now - SebKeyCapture.TouchExitSequenceStartedTime < new TimeSpan(0, 0, 2)))
          return;
        int y1 = cursorsPosition.Y;
        workingArea = Screen.PrimaryScreen.WorkingArea;
        int num3 = workingArea.Height / 3;
        if (y1 <= num3)
          return;
        int y2 = cursorsPosition.Y;
        workingArea = Screen.PrimaryScreen.WorkingArea;
        int num4 = workingArea.Height / 3 * 2;
        if (y2 >= num4)
          return;
        SEBClientInfo.SebWindowsClientForm.ShowCloseDialogForm();
      }
    }

    private static void TestAppExitSequences(IntPtr wp, IntPtr lp)
    {
      if (wp == (IntPtr) 256)
      {
        if (SebKeyCapture.SetAndTestCtrlQExitSequence(wp, lp))
          SEBClientInfo.SebWindowsClientForm.ShowCloseDialogForm();
        if (!SebKeyCapture.SetAndTestExitKeySequence(wp, lp) || (bool) SEBSettings.valueForDictionaryKey(SEBSettings.settingsCurrent, "ignoreExitKeys"))
          return;
        SEBClientInfo.SebWindowsClientForm.ExitApplication(true);
      }
      else
      {
        if (!(wp == (IntPtr) 257))
          return;
        SebKeyCapture.ResetCtrlQExitSequence(wp, lp);
        SebKeyCapture.ResetExitKeySequence(wp, lp);
      }
    }

    private static IntPtr CaptureKey(int nCode, IntPtr wp, IntPtr lp)
    {
      if (nCode >= 0)
      {
        SebKeyCapture.TestAppExitSequences(wp, lp);
        SebKeyCapture.KBDLLHOOKSTRUCT structure = (SebKeyCapture.KBDLLHOOKSTRUCT) Marshal.PtrToStructure(lp, typeof (SebKeyCapture.KBDLLHOOKSTRUCT));
        if ((bool) SEBClientInfo.getSebSetting("enableAltTab")["enableAltTab"])
        {
          if (structure.key == Keys.Tab && structure.flags == 32 && (int) wp == 260)
          {
            if (SebKeyCapture.Tab_Pressed_First_Time && (int) wp == 260)
            {
              SEBClientInfo.SebWindowsClientForm.ShowApplicationChooserForm();
              if (SebKeyCapture.Tab_Pressed_First_Time)
                SebKeyCapture.Tab_Pressed_First_Time = false;
              return (IntPtr) 1;
            }
            if (!SebKeyCapture.Tab_Pressed_First_Time && (int) wp == 260)
            {
              SEBClientInfo.SebWindowsClientForm.SelectNextListItem();
              return (IntPtr) 1;
            }
          }
          if ((structure.key == Keys.LMenu && structure.flags == 128 || structure.key == Keys.RMenu && structure.flags == 129) && (int) wp == 257)
          {
            SEBClientInfo.SebWindowsClientForm.HideApplicationChooserForm();
            SebKeyCapture.Tab_Pressed_First_Time = true;
          }
        }
        if (SebKeyCapture.DisableKey(wp, lp))
          return (IntPtr) 1;
      }
      return SebKeyCapture.CallNextHookEx(SebKeyCapture.ptrKeyboardHook, nCode, wp, lp);
    }

    private static void RegisterKeyboardHookMethod()
    {
      ProcessModule mainModule = Process.GetCurrentProcess().MainModule;
      SebKeyCapture.objKeyboardProcess = new SebKeyCapture.LowLevelProc(SebKeyCapture.CaptureKey);
      SebKeyCapture.objMouseProcess = new SebKeyCapture.LowLevelProc(SebKeyCapture.CaptureMouseButton);
      SebKeyCapture.ptrKeyboardHook = SebKeyCapture.SetWindowsHookEx(13, SebKeyCapture.objKeyboardProcess, SebKeyCapture.GetModuleHandle(mainModule.ModuleName), 0U);
      SebKeyCapture.ptrMouseHook = SebKeyCapture.SetWindowsHookEx(14, SebKeyCapture.objMouseProcess, SebKeyCapture.GetModuleHandle(mainModule.ModuleName), 0U);
    }

    private static void UnregisterKeyboardHookMethod()
    {
      if (SebKeyCapture.ptrKeyboardHook != IntPtr.Zero)
      {
        SebKeyCapture.UnhookWindowsHookEx(SebKeyCapture.ptrKeyboardHook);
        SebKeyCapture.ptrKeyboardHook = IntPtr.Zero;
      }
      if (!(SebKeyCapture.ptrMouseHook != IntPtr.Zero))
        return;
      SebKeyCapture.UnhookWindowsHookEx(SebKeyCapture.ptrMouseHook);
      SebKeyCapture.ptrMouseHook = IntPtr.Zero;
    }

    private enum MouseMessages
    {
      WM_MOUSEMOVE = 512,
      WM_LBUTTONDOWN = 513,
      WM_LBUTTONUP = 514,
      WM_RBUTTONDOWN = 516,
      WM_RBUTTONUP = 517,
      WM_MOUSEWHEEL = 522,
    }

    private struct POINT
    {
      public int x;
      public int y;
    }

    private struct MSLLHOOKSTRUCT
    {
      public SebKeyCapture.POINT pt;
      public uint mouseData;
      public uint flags;
      public uint time;
      public IntPtr dwExtraInfo;
    }

    private struct KBDLLHOOKSTRUCT
    {
      public Keys key;
      public int scanCode;
      public int flags;
      public int time;
      public IntPtr extra;
    }

    private delegate IntPtr LowLevelProc(int nCode, IntPtr wParam, IntPtr lParam);
  }
}
