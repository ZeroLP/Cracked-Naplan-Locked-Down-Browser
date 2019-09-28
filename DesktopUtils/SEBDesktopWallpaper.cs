
using System.Runtime.InteropServices;

namespace SebWindowsClient.DesktopUtils
{
  public static class SEBDesktopWallpaper
  {
    private const int SPI_SETDESKWALLPAPER = 20;
    private const int SPIF_UPDATEINIFILE = 1;
    private const int SPIF_SENDWININICHANGE = 2;
    private const int SPI_GETDESKWALLPAPER = 115;
    private const int MAX_PATH = 260;
    private static string _currentWallpaper;

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

    public static void BlankWallpaper()
    {
    }

    public static void Reset()
    {
    }

    private static string GetWallpaper()
    {
      string lpvParam = new string(char.MinValue, 260);
      SEBDesktopWallpaper.SystemParametersInfo(115, lpvParam.Length, lpvParam, 0);
      return lpvParam.Substring(0, lpvParam.IndexOf(char.MinValue));
    }

    private static void SetWallpaper(string path)
    {
      SEBDesktopWallpaper.SystemParametersInfo(20, 0, path, 3);
    }

    public enum Style
    {
      Tiled,
      Centered,
      Stretched,
    }
  }
}
