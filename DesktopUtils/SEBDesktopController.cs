

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SebWindowsClient.DesktopUtils
{
  public class SEBDesktopController : IDisposable, ICloneable
  {
    private static int SPI_SETSCREENSAVERRUNNING = 97;
    private static int TRUE = 1;
    private static int NULL = 0;
    public static readonly SEBDesktopController Default = SEBDesktopController.OpenDefaultDesktop();
    public static readonly SEBDesktopController Input = SEBDesktopController.OpenInputDesktop();
    public const int MaxWindowNameLength = 100;
    private const short SW_HIDE = 0;
    private const short SW_NORMAL = 1;
    private const int STARTF_USESTDHANDLES = 256;
    private const int STARTF_USESHOWWINDOW = 1;
    private const int UOI_NAME = 2;
    private const int STARTF_USEPOSITION = 4;
    private const int NORMAL_PRIORITY_CLASS = 32;
    private const int DESKTOP_CREATEWINDOW = 2;
    private const int DESKTOP_ENUMERATE = 64;
    private const int DESKTOP_WRITEOBJECTS = 128;
    private const int DESKTOP_SWITCHDESKTOP = 256;
    private const int DESKTOP_CREATEMENU = 4;
    private const int DESKTOP_HOOKCONTROL = 8;
    private const int DESKTOP_READOBJECTS = 1;
    private const int DESKTOP_JOURNALRECORD = 16;
    private const int DESKTOP_JOURNALPLAYBACK = 32;
    private const uint AccessRights = 511;
    private IntPtr m_desktop;
    private string m_desktopName;
    private static StringCollection m_sc;
    private ArrayList m_windows;
    private bool m_disposed;

    [DllImport("kernel32.dll")]
    private static extern int GetThreadId(IntPtr thread);

    [DllImport("kernel32.dll")]
    private static extern int GetProcessId(IntPtr process);

    [DllImport("user32.dll")]
    private static extern IntPtr CreateDesktop(string lpszDesktop, IntPtr lpszDevice, IntPtr pDevmode, int dwFlags, uint dwDesiredAccess, IntPtr lpsa);

    [DllImport("user32.dll")]
    private static extern bool CloseDesktop(IntPtr hDesktop);

    [DllImport("user32.dll")]
    private static extern IntPtr OpenDesktop(string lpszDesktop, uint dwFlags, bool fInherit, uint dwDesiredAccess);

    [DllImport("user32.dll")]
    private static extern IntPtr OpenInputDesktop(uint dwFlags, bool fInherit, uint dwDesiredAccess);

    [DllImport("user32.dll")]
    private static extern bool SwitchDesktop(IntPtr hDesktop);

    [DllImport("user32.dll")]
    private static extern bool EnumDesktops(IntPtr hwinsta, SEBDesktopController.EnumDesktopProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern IntPtr GetProcessWindowStation();

    [DllImport("user32.dll")]
    private static extern bool EnumDesktopWindows(IntPtr hDesktop, SEBDesktopController.EnumDesktopWindowsProc lpfn, IntPtr lParam);

    [DllImport("user32.dll")]
    public static extern bool SetThreadDesktop(IntPtr hDesktop);

    [DllImport("user32.dll")]
    private static extern IntPtr GetThreadDesktop(int dwThreadId);

    [DllImport("user32.dll")]
    private static extern bool GetUserObjectInformation(IntPtr hObj, int nIndex, IntPtr pvInfo, int nLength, ref int lpnLengthNeeded);

    [DllImport("kernel32.dll")]
    private static extern bool CreateProcess(string lpApplicationName, string lpCommandLine, IntPtr lpProcessAttributes, IntPtr lpThreadAttributes, bool bInheritHandles, int dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, ref SEBDesktopController.STARTUPINFO lpStartupInfo, ref SEBDesktopController.PROCESS_INFORMATION lpProcessInformation);

    [DllImport("user32.dll")]
    private static extern int GetWindowText(IntPtr hWnd, IntPtr lpString, int nMaxCount);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool SystemParametersInfo(int uiAction, int uiParam, IntPtr pvParam, int fWinIni);

    public bool IsOpen
    {
      get
      {
        return this.m_desktop != IntPtr.Zero;
      }
    }

    public string DesktopName
    {
      get
      {
        return this.m_desktopName;
      }
    }

    public IntPtr DesktopHandle
    {
      get
      {
        return this.m_desktop;
      }
    }

    public SEBDesktopController()
    {
      this.m_desktop = IntPtr.Zero;
      this.m_desktopName = string.Empty;
      this.m_windows = new ArrayList();
      this.m_disposed = false;
    }

    private SEBDesktopController(IntPtr desktop)
    {
      this.m_desktop = desktop;
      this.m_desktopName = SEBDesktopController.GetDesktopName(desktop);
      this.m_windows = new ArrayList();
      this.m_disposed = false;
    }

    ~SEBDesktopController()
    {
      this.Close();
    }

    public bool Create(string name)
    {
      this.CheckDisposed();
      if (this.m_desktop != IntPtr.Zero && !this.Close())
        return false;
      if (SEBDesktopController.Exists(name))
        return this.Open(name);
      this.m_desktop = SEBDesktopController.CreateDesktop(name, IntPtr.Zero, IntPtr.Zero, 0, 511U, IntPtr.Zero);
      this.m_desktopName = name;
      return !(this.m_desktop == IntPtr.Zero);
    }

    public bool Close()
    {
      this.CheckDisposed();
      if (!(this.m_desktop != IntPtr.Zero))
        return true;
      int num = SEBDesktopController.CloseDesktop(this.m_desktop) ? 1 : 0;
      if (num == 0)
        return num != 0;
      this.m_desktop = IntPtr.Zero;
      this.m_desktopName = string.Empty;
      return num != 0;
    }

    public bool Open(string name)
    {
      this.CheckDisposed();
      if (this.m_desktop != IntPtr.Zero && !this.Close())
        return false;
      this.m_desktop = SEBDesktopController.OpenDesktop(name, 0U, true, 511U);
      if (this.m_desktop == IntPtr.Zero)
        return false;
      this.m_desktopName = name;
      return true;
    }

    public bool OpenInput()
    {
      this.CheckDisposed();
      if (this.m_desktop != IntPtr.Zero && !this.Close())
        return false;
      this.m_desktop = SEBDesktopController.OpenInputDesktop(0U, true, 511U);
      if (this.m_desktop == IntPtr.Zero)
        return false;
      this.m_desktopName = SEBDesktopController.GetDesktopName(this.m_desktop);
      return true;
    }

    public bool Show()
    {
      this.CheckDisposed();
      if (this.m_desktop == IntPtr.Zero)
        return false;
      return SEBDesktopController.SwitchDesktop(this.m_desktop);
    }

    public SEBDesktopController.WindowCollection GetWindows()
    {
      this.CheckDisposed();
      if (!this.IsOpen)
        return (SEBDesktopController.WindowCollection) null;
      this.m_windows.Clear();
      SEBDesktopController.WindowCollection windowCollection1 = new SEBDesktopController.WindowCollection();
      if (!SEBDesktopController.EnumDesktopWindows(this.m_desktop, new SEBDesktopController.EnumDesktopWindowsProc(this.DesktopWindowsProc), IntPtr.Zero))
        return (SEBDesktopController.WindowCollection) null;
      SEBDesktopController.WindowCollection windowCollection2 = new SEBDesktopController.WindowCollection();
      IntPtr num = Marshal.AllocHGlobal(100);
      foreach (IntPtr window in this.m_windows)
      {
        SEBDesktopController.GetWindowText(window, num, 100);
        windowCollection2.Add(new SEBDesktopController.Window(window, Marshal.PtrToStringAnsi(num)));
      }
      Marshal.FreeHGlobal(num);
      return windowCollection2;
    }

    private bool DesktopWindowsProc(IntPtr wndHandle, IntPtr lParam)
    {
      this.m_windows.Add((object) wndHandle);
      return true;
    }

    public Process CreateProcess(string path)
    {
      this.CheckDisposed();
      if (!this.IsOpen)
        return (Process) null;
      SEBDesktopController.STARTUPINFO lpStartupInfo = new SEBDesktopController.STARTUPINFO();
      lpStartupInfo.cb = Marshal.SizeOf((object) lpStartupInfo);
      lpStartupInfo.lpDesktop = this.m_desktopName;
      SEBDesktopController.PROCESS_INFORMATION lpProcessInformation = new SEBDesktopController.PROCESS_INFORMATION();
      if (!SEBDesktopController.CreateProcess((string) null, path, IntPtr.Zero, IntPtr.Zero, true, 32, IntPtr.Zero, (string) null, ref lpStartupInfo, ref lpProcessInformation))
        return (Process) null;
      return Process.GetProcessById(lpProcessInformation.dwProcessId);
    }

    public void Prepare()
    {
      this.CheckDisposed();
      if (!this.IsOpen)
        return;
      this.CreateProcess("explorer.exe");
    }

    public static string[] GetDesktops()
    {
      IntPtr processWindowStation = SEBDesktopController.GetProcessWindowStation();
      if (processWindowStation == IntPtr.Zero)
        return new string[0];
      string[] strArray;
      lock (SEBDesktopController.m_sc = new StringCollection())
      {
        if (!SEBDesktopController.EnumDesktops(processWindowStation, new SEBDesktopController.EnumDesktopProc(SEBDesktopController.DesktopProc), IntPtr.Zero))
          return new string[0];
        strArray = new string[SEBDesktopController.m_sc.Count];
        for (int index = 0; index < strArray.Length; ++index)
          strArray[index] = SEBDesktopController.m_sc[index];
      }
      return strArray;
    }

    private static bool DesktopProc(string lpszDesktop, IntPtr lParam)
    {
      SEBDesktopController.m_sc.Add(lpszDesktop);
      return true;
    }

    public static bool Show(string name)
    {
      using (SEBDesktopController desktopController = new SEBDesktopController())
      {
        if (!desktopController.Open(name))
          return false;
        return desktopController.Show();
      }
    }

    public static SEBDesktopController GetCurrent()
    {
      return new SEBDesktopController(SEBDesktopController.GetThreadDesktop(AppDomain.GetCurrentThreadId()));
    }

    public static bool SetCurrent(SEBDesktopController desktop)
    {
      if (desktop == null || !desktop.IsOpen)
        return false;
      return SEBDesktopController.SetThreadDesktop(desktop.DesktopHandle);
    }

    public static SEBDesktopController OpenDesktop(string name)
    {
      SEBDesktopController desktopController = new SEBDesktopController();
      if (!desktopController.Open(name))
        return (SEBDesktopController) null;
      return desktopController;
    }

    public static SEBDesktopController OpenInputDesktop()
    {
      SEBDesktopController desktopController = new SEBDesktopController();
      if (!desktopController.OpenInput())
        return (SEBDesktopController) null;
      return desktopController;
    }

    public static SEBDesktopController OpenDefaultDesktop()
    {
      return SEBDesktopController.OpenDesktop("Default");
    }

    public static SEBDesktopController CreateDesktop(string name)
    {
      SEBDesktopController desktopController = new SEBDesktopController();
      if (!desktopController.Create(name))
        return (SEBDesktopController) null;
      return desktopController;
    }

    public static string GetDesktopName(SEBDesktopController desktop)
    {
      if (desktop.IsOpen)
        return (string) null;
      return SEBDesktopController.GetDesktopName(desktop.DesktopHandle);
    }

    public static string GetDesktopName(IntPtr desktopHandle)
    {
      if (desktopHandle == IntPtr.Zero)
        return (string) null;
      int lpnLengthNeeded = 0;
      string empty = string.Empty;
      SEBDesktopController.GetUserObjectInformation(desktopHandle, 2, IntPtr.Zero, 0, ref lpnLengthNeeded);
      IntPtr num1 = Marshal.AllocHGlobal(lpnLengthNeeded);
      int num2 = SEBDesktopController.GetUserObjectInformation(desktopHandle, 2, num1, lpnLengthNeeded, ref lpnLengthNeeded) ? 1 : 0;
      string stringAnsi = Marshal.PtrToStringAnsi(num1);
      Marshal.FreeHGlobal(num1);
      if (num2 == 0)
        return (string) null;
      return stringAnsi;
    }

    public static bool Exists(string name)
    {
      return SEBDesktopController.Exists(name, false);
    }

    public static bool Exists(string name, bool caseInsensitive)
    {
      foreach (string desktop in SEBDesktopController.GetDesktops())
      {
        if (caseInsensitive)
        {
          if (desktop.ToLower() == name.ToLower())
            return true;
        }
        else if (desktop == name)
          return true;
      }
      return false;
    }

    public static Process CreateProcess(string path, string desktop)
    {
      if (!SEBDesktopController.Exists(desktop))
        return (Process) null;
      return SEBDesktopController.OpenDesktop(desktop).CreateProcess(path);
    }

    public static SEBDesktopController.WindowCollection GetWindows(string desktop)
    {
      if (!SEBDesktopController.Exists(desktop))
        return (SEBDesktopController.WindowCollection) null;
      return SEBDesktopController.OpenDesktop(desktop).GetWindows();
    }

    public static Process[] GetInputProcesses()
    {
      Process[] processes = Process.GetProcesses();
      ArrayList arrayList = new ArrayList();
      string desktopName = SEBDesktopController.GetDesktopName(SEBDesktopController.Input.DesktopHandle);
      foreach (Process process in processes)
      {
        foreach (ProcessThread thread in (ReadOnlyCollectionBase) process.Threads)
        {
          if (SEBDesktopController.GetDesktopName(SEBDesktopController.GetThreadDesktop(thread.Id)) == desktopName)
          {
            arrayList.Add((object) process);
            break;
          }
        }
      }
      Process[] processArray = new Process[arrayList.Count];
      for (int index = 0; index < processArray.Length; ++index)
        processArray[index] = (Process) arrayList[index];
      return processArray;
    }

    public static Process[] GetInputProcessesWithGI()
    {
      Process[] processes = Process.GetProcesses();
      ArrayList arrayList = new ArrayList();
      SEBDesktopController.GetDesktopName(SEBDesktopController.Input.DesktopHandle);
      foreach (Process process in processes)
      {
        if (process.MainWindowTitle.Length > 0)
          arrayList.Add((object) process);
      }
      Process[] processArray = new Process[arrayList.Count];
      for (int index = 0; index < processArray.Length; ++index)
        processArray[index] = (Process) arrayList[index];
      return processArray;
    }

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    public virtual void Dispose(bool disposing)
    {
      if (!this.m_disposed)
        this.Close();
      this.m_disposed = true;
    }

    private void CheckDisposed()
    {
      if (this.m_disposed)
        throw new ObjectDisposedException("");
    }

    public object Clone()
    {
      this.CheckDisposed();
      SEBDesktopController desktopController = new SEBDesktopController();
      if (this.IsOpen)
        desktopController.Open(this.m_desktopName);
      return (object) desktopController;
    }

    public static bool DisableTaskSwitching()
    {
      IntPtr pvParam = new IntPtr(0);
      return SEBDesktopController.SystemParametersInfo(SEBDesktopController.SPI_SETSCREENSAVERRUNNING, SEBDesktopController.TRUE, pvParam, SEBDesktopController.NULL);
    }

    public override string ToString()
    {
      return this.m_desktopName;
    }

    private delegate bool EnumDesktopProc(string lpszDesktop, IntPtr lParam);

    private delegate bool EnumDesktopWindowsProc(IntPtr desktopHandle, IntPtr lParam);

    private struct PROCESS_INFORMATION
    {
      public IntPtr hProcess;
      public IntPtr hThread;
      public int dwProcessId;
      public int dwThreadId;
    }

    private struct STARTUPINFO
    {
      public int cb;
      public string lpReserved;
      public string lpDesktop;
      public string lpTitle;
      public int dwX;
      public int dwY;
      public int dwXSize;
      public int dwYSize;
      public int dwXCountChars;
      public int dwYCountChars;
      public int dwFillAttribute;
      public int dwFlags;
      public short wShowWindow;
      public short cbReserved2;
      public IntPtr lpReserved2;
      public IntPtr hStdInput;
      public IntPtr hStdOutput;
      public IntPtr hStdError;
    }

    public struct Window
    {
      private IntPtr m_handle;
      private string m_text;

      public IntPtr Handle
      {
        get
        {
          return this.m_handle;
        }
      }

      public string Text
      {
        get
        {
          return this.m_text;
        }
      }

      public Window(IntPtr handle, string text)
      {
        this.m_handle = handle;
        this.m_text = text;
      }
    }

    public class WindowCollection : CollectionBase
    {
      public SEBDesktopController.Window this[int index]
      {
        get
        {
          return (SEBDesktopController.Window) this.List[index];
        }
      }

      public void Add(SEBDesktopController.Window wnd)
      {
        this.List.Add((object) wnd);
      }
    }
  }
}
