
using SebWindowsClient.ConfigurationUtils;
using System;
using System.Windows.Forms;

namespace SebWindowsClient.UI
{
  public class SEBToolStripButton : ToolStripButton
  {
    public SEBToolStripButton()
    {
      this.ImageScaling = ToolStripItemImageScaling.SizeToFit;
    }

    public string Identifier { get; set; }

    public string WindowHandlingProcess { get; set; }

    public int FontSize
    {
      get
      {
        float num = (float) (10.0 * ((double) (int) SEBSettings.valueForDictionaryKey(SEBSettings.settingsCurrent, "taskBarHeight") / 40.0)) * SEBClientInfo.scaleFactor;
        if ((bool) SEBClientInfo.getSebSetting("touchOptimized")["touchOptimized"])
          return (int) Math.Round(1.7 * (double) num);
        return (int) Math.Round((double) num);
      }
    }

    protected override void OnMouseHover(EventArgs e)
    {
      if (this.Parent != null)
        this.Parent.Focus();
      base.OnMouseHover(e);
    }
  }
}
