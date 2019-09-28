
using SebWindowsClient.DiagnosticsUtils;
using System;
using System.Runtime.InteropServices;

namespace SebWindowsClient.ProcessUtils
{
  public class SEBClipboard
  {
    [DllImport("user32.dll")]
    private static extern bool EmptyClipboard();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool OpenClipboard(IntPtr hWndNewOwner);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool CloseClipboard();

    public static void CleanClipboard()
    {
      try
      {
        SEBClipboard.OpenClipboard(IntPtr.Zero);
        SEBClipboard.EmptyClipboard();
        SEBClipboard.CloseClipboard();
      }
      catch (Exception ex)
      {
        Logger.AddError("Error ocurred by cleaning Clipboard.", (object) null, ex, ex.Message);
      }
    }
  }
}
