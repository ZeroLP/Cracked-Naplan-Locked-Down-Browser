

using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace SebWindowsClient.Properties
{
  [GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
  [DebuggerNonUserCode]
  [CompilerGenerated]
  internal class Resources
  {
    private static ResourceManager resourceMan;
    private static CultureInfo resourceCulture;

    internal Resources()
    {
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    internal static ResourceManager ResourceManager
    {
      get
      {
        if (SebWindowsClient.Properties.Resources.resourceMan == null)
          SebWindowsClient.Properties.Resources.resourceMan = new ResourceManager("SebWindowsClient.Properties.Resources", typeof (SebWindowsClient.Properties.Resources).Assembly);
        return SebWindowsClient.Properties.Resources.resourceMan;
      }
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    internal static CultureInfo Culture
    {
      get
      {
        return SebWindowsClient.Properties.Resources.resourceCulture;
      }
      set
      {
        SebWindowsClient.Properties.Resources.resourceCulture = value;
      }
    }

    internal static Bitmap battery
    {
      get
      {
        return (Bitmap) SebWindowsClient.Properties.Resources.ResourceManager.GetObject(nameof (battery), SebWindowsClient.Properties.Resources.resourceCulture);
      }
    }

    internal static Bitmap closewindow
    {
      get
      {
        return (Bitmap) SebWindowsClient.Properties.Resources.ResourceManager.GetObject(nameof (closewindow), SebWindowsClient.Properties.Resources.resourceCulture);
      }
    }

    internal static Bitmap keyboard
    {
      get
      {
        return (Bitmap) SebWindowsClient.Properties.Resources.ResourceManager.GetObject(nameof (keyboard), SebWindowsClient.Properties.Resources.resourceCulture);
      }
    }

    internal static Bitmap loading
    {
      get
      {
        return (Bitmap) SebWindowsClient.Properties.Resources.ResourceManager.GetObject(nameof (loading), SebWindowsClient.Properties.Resources.resourceCulture);
      }
    }

    internal static Bitmap quit
    {
      get
      {
        return (Bitmap) SebWindowsClient.Properties.Resources.ResourceManager.GetObject(nameof (quit), SebWindowsClient.Properties.Resources.resourceCulture);
      }
    }

    internal static Bitmap refresh
    {
      get
      {
        return (Bitmap) SebWindowsClient.Properties.Resources.ResourceManager.GetObject(nameof (refresh), SebWindowsClient.Properties.Resources.resourceCulture);
      }
    }

    internal static Bitmap reload
    {
      get
      {
        return (Bitmap) SebWindowsClient.Properties.Resources.ResourceManager.GetObject(nameof (reload), SebWindowsClient.Properties.Resources.resourceCulture);
      }
    }

    internal static Bitmap restartExam
    {
      get
      {
        return (Bitmap) SebWindowsClient.Properties.Resources.ResourceManager.GetObject(nameof (restartExam), SebWindowsClient.Properties.Resources.resourceCulture);
      }
    }

    internal static Bitmap SebGlobalDialogImage
    {
      get
      {
        return (Bitmap) SebWindowsClient.Properties.Resources.ResourceManager.GetObject(nameof (SebGlobalDialogImage), SebWindowsClient.Properties.Resources.resourceCulture);
      }
    }

    internal static string SplashScreenTextTpl
    {
      get
      {
        return SebWindowsClient.Properties.Resources.ResourceManager.GetString(nameof (SplashScreenTextTpl), SebWindowsClient.Properties.Resources.resourceCulture);
      }
    }

    internal static Bitmap wlan0
    {
      get
      {
        return (Bitmap) SebWindowsClient.Properties.Resources.ResourceManager.GetObject(nameof (wlan0), SebWindowsClient.Properties.Resources.resourceCulture);
      }
    }

    internal static Bitmap wlan100
    {
      get
      {
        return (Bitmap) SebWindowsClient.Properties.Resources.ResourceManager.GetObject(nameof (wlan100), SebWindowsClient.Properties.Resources.resourceCulture);
      }
    }

    internal static Bitmap wlan33
    {
      get
      {
        return (Bitmap) SebWindowsClient.Properties.Resources.ResourceManager.GetObject(nameof (wlan33), SebWindowsClient.Properties.Resources.resourceCulture);
      }
    }

    internal static Bitmap wlan66
    {
      get
      {
        return (Bitmap) SebWindowsClient.Properties.Resources.ResourceManager.GetObject(nameof (wlan66), SebWindowsClient.Properties.Resources.resourceCulture);
      }
    }

    internal static Bitmap wlannointerface
    {
      get
      {
        return (Bitmap) SebWindowsClient.Properties.Resources.ResourceManager.GetObject(nameof (wlannointerface), SebWindowsClient.Properties.Resources.resourceCulture);
      }
    }
  }
}
