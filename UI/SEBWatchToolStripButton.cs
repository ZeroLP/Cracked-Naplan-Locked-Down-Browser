
using System;
using System.Drawing;
using System.Windows.Forms;

namespace SebWindowsClient.UI
{
  public class SEBWatchToolStripButton : SEBToolStripButton
  {
    private Timer timer;

    public SEBWatchToolStripButton()
    {
      this.InitializeComponent();
      this.timer = new Timer();
      this.timer.Tick += new EventHandler(this.OnTimerTick);
      this.timer.Interval = 10000;
      this.timer.Start();
      this.Update();
    }

    private void OnTimerTick(object sender, EventArgs e)
    {
      this.Update();
    }

    private void Update()
    {
      this.Text = DateTime.Now.ToShortTimeString();
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
      this.Alignment = ToolStripItemAlignment.Right;
      this.ForeColor = Color.Black;
      this.Font = new Font("Arial", (float) this.FontSize, FontStyle.Bold);
      this.Enabled = false;
    }
  }
}
