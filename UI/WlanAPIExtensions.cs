

using SebWindowsClient.WlanUtils;
using System.Text;

namespace SebWindowsClient.UI
{
  public static class WlanAPIExtensions
  {
    public static string GetSSID(this Wlan.Dot11Ssid ssid)
    {
      return Encoding.ASCII.GetString(ssid.SSID, 0, (int) ssid.SSIDLength);
    }
  }
}
