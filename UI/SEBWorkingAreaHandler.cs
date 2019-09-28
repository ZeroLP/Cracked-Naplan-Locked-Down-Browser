

using SebWindowsClient.DiagnosticsUtils;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SebWindowsClient.UI
{
  public static class SEBWorkingAreaHandler
  {
    private const int SPIF_SENDWININICHANGE = 2;
    private const int SPIF_UPDATEINIFILE = 1;
    private const int SPIF_change = 3;
    private const int SPI_SETWORKAREA = 47;
    private const int SPI_GETWORKAREA = 48;
    private static bool _originalWorkingAreaSet;
    private static SEBWorkingAreaHandler.RECT _originalWorkingArea;

    public static void SetTaskBarSpaceHeight(int taskbarHeight)
    {
      if (!SEBWorkingAreaHandler._originalWorkingAreaSet)
      {
        SEBWorkingAreaHandler._originalWorkingArea.Bottom = Screen.PrimaryScreen.WorkingArea.Bottom;
        SEBWorkingAreaHandler._originalWorkingArea.Left = Screen.PrimaryScreen.WorkingArea.Left;
        SEBWorkingAreaHandler._originalWorkingArea.Right = Screen.PrimaryScreen.WorkingArea.Right;
        SEBWorkingAreaHandler._originalWorkingArea.Top = Screen.PrimaryScreen.WorkingArea.Top;
        SEBWorkingAreaHandler._originalWorkingAreaSet = true;
      }
      SEBWorkingAreaHandler.SetWorkspace(new SEBWorkingAreaHandler.RECT()
      {
        Bottom = Screen.PrimaryScreen.Bounds.Height - taskbarHeight,
        Left = 0,
        Right = Screen.PrimaryScreen.Bounds.Width,
        Top = 0
      });
    }

    public static void ResetWorkspaceArea()
    {
      if (!SEBWorkingAreaHandler._originalWorkingAreaSet)
        return;
      SEBWorkingAreaHandler.SetWorkspace(SEBWorkingAreaHandler._originalWorkingArea);
    }

    private static bool SetWorkspace(SEBWorkingAreaHandler.RECT rect)
    {
      try
      {
        return SEBWorkingAreaHandler.SystemParametersInfo(47, (int) IntPtr.Zero, ref rect, 3);
      }
      catch (Exception ex)
      {
        Logger.AddError("Unable to set Working Area", (object) null, ex, (string) null);
        return false;
      }
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SystemParametersInfo(int uiAction, int uiParam, ref SEBWorkingAreaHandler.RECT pvParam, int fWinIni);

    public struct RECT
    {
      public int Left;
      public int Top;
      public int Right;
      public int Bottom;
    }
  }
}
