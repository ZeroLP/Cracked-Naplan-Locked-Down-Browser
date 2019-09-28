

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SebWindowsClient.ProcessUtils
{
  public class ForegroundWatchDog
  {
    private List<IntPtr> _hooks = new List<IntPtr>();
    private ForegroundWatchDog.WinEventDelegate dele;
    private const uint WINEVENT_OUTOFCONTEXT = 0;
    private const uint EVENT_SYSTEM_FOREGROUND = 3;
    private const uint EVENT_SYSTEM_MINIMIZEEND = 16;
    private const uint EVENT_SYSTEM_MOVESIZEEND = 10;
    private const uint EVENT_SYSTEM_SWITCHEND = 21;
    private const uint EVENT_SYSTEM_CAPTURESTART = 8;
    private const uint EVENT_SYSTEM_SWITCHSTART = 20;
    private bool running;

    public event ForegroundWatchDog.ForegroundWindowChangedEventHandler OnForegroundWindowChanged;

    public ForegroundWatchDog()
    {
      if (this.dele == null)
        this.dele = new ForegroundWatchDog.WinEventDelegate(this.WinEventProc);
      ForegroundWatchDog.SetWinEventHook(3U, 3U, IntPtr.Zero, this.dele, 0U, 0U, 0U);
      ForegroundWatchDog.SetWinEventHook(8U, 8U, IntPtr.Zero, this.dele, 0U, 0U, 0U);
    }

    public void StartWatchDog()
    {
      this.running = true;
    }

    public void StopWatchDog()
    {
      this.running = false;
    }

    public void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
    {
      // ISSUE: reference to a compiler-generated field
      if (this.OnForegroundWindowChanged != null && hwnd != IntPtr.Zero)
      {
        // ISSUE: reference to a compiler-generated field
        this.OnForegroundWindowChanged(hwnd);
      }
      if (!this.running)
        return;
      Console.WriteLine((object) hwnd);
      if (hwnd == IntPtr.Zero || hwnd.IsAllowed())
        return;
      hwnd.HideWindow();
    }

    [DllImport("user32.dll")]
    private static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, ForegroundWatchDog.WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

    [DllImport("user32.dll")]
    public static extern uint GetWindowThreadProcessId(IntPtr hwnd, out uint lpdwProcessId);

    public delegate void ForegroundWindowChangedEventHandler(IntPtr windowHandle);

    private delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);
  }
}
