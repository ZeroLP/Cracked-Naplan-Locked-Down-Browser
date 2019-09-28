
using SebWindowsClient.Properties;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace SebWindowsClient.UI
{
  public class SEBBatterylifeToolStripButton : SEBToolStripButton
  {
    private PowerStatus powerStatus = SystemInformation.PowerStatus;
    private Timer timer;

    public SEBBatterylifeToolStripButton()
    {
      this.InitializeComponent();
      this.timer = new Timer();
      this.timer.Tick += new EventHandler(this.OnTimerTick);
      this.timer.Interval = 5000;
      this.timer.Start();
      this.Update();
    }

    private void OnTimerTick(object sender, EventArgs e)
    {
      this.Update();
    }

    private void Update()
    {
      this.Visible = SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Offline;
      this.Text = string.Format(" {0}%", (object) (float) ((double) this.powerStatus.BatteryLifePercent * 100.0));
    }

    protected override void Dispose(bool disposing)
    {
      this.timer.Tick -= new EventHandler(this.OnTimerTick);
      this.timer.Stop();
      this.timer = (Timer) null;
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      this.Image = (Image) Resources.ResourceManager.GetObject("battery");
      this.TextImageRelation = TextImageRelation.Overlay;
      this.Alignment = ToolStripItemAlignment.Right;
      this.ForeColor = Color.Black;
      this.Font = new Font("Arial", (float) (int) ((double) this.FontSize * 0.7), FontStyle.Bold);
      this.Enabled = false;
    }
  }
}
