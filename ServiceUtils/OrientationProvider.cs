

using System;
using System.Threading;
using System.Windows.Forms;

namespace SebWindowsClient.ServiceUtils
{
  public static class OrientationProvider
  {
    private static readonly System.Threading.Timer _timer = new System.Threading.Timer((TimerCallback) (s =>
    {
      int num;
      switch (SystemInformation.ScreenOrientation)
      {
        case ScreenOrientation.Angle0:
        case ScreenOrientation.Angle180:
          num = 0;
          break;
        default:
          num = 1;
          break;
      }
      OrientationProvider.Current = (OrientationProvider.Orientation) num;
    }), (object) null, 0, 100);
    private static OrientationProvider.Orientation _current;

    public static event EventHandler<OrientationProvider.Orientation> Changed;

    public static OrientationProvider.Orientation Current
    {
      get
      {
        return OrientationProvider._current;
      }
      private set
      {
        if (OrientationProvider._current == value)
          return;
        OrientationProvider._current = value;
        EventHandler<OrientationProvider.Orientation> changed = OrientationProvider.Changed;
        if (changed == null)
          return;
        object local = null;
        int current = (int) OrientationProvider._current;
        changed((object) local, (OrientationProvider.Orientation) current);
      }
    }

    public enum Orientation
    {
      Landscape,
      Portrait,
    }
  }
}
