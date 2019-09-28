

using SebWindowsClient.ServiceUtils;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SebWindowsClient
{
  public class OrientationLockForm : Form
  {
    private IContainer components;
    private Label messageLabel;

    public OrientationLockForm()
    {
      this.InitializeComponent();
    }

    protected override void OnShown(EventArgs e)
    {
      base.OnShown(e);
      OrientationProvider.Changed += new EventHandler<OrientationProvider.Orientation>(this.OrientationProvider_Changed);
      this.OrientationProvider_Changed((object) null, OrientationProvider.Current);
    }

    private void OrientationProvider_Changed(object sender, OrientationProvider.Orientation orientation)
    {
      if (orientation != OrientationProvider.Orientation.Landscape)
        return;
      OrientationProvider.Changed -= new EventHandler<OrientationProvider.Orientation>(this.OrientationProvider_Changed);
      this.BeginInvoke((Delegate) new MethodInvoker(((Form) this).Close));
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      this.messageLabel = new Label();
      this.SuspendLayout();
      this.messageLabel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
      this.messageLabel.Font = new Font("Microsoft Sans Serif", 16f, FontStyle.Regular, GraphicsUnit.Point, (byte) 204);
      this.messageLabel.ForeColor = Color.Red;
      this.messageLabel.Location = new Point(9, 9);
      this.messageLabel.Margin = new Padding(0);
      this.messageLabel.Name = "messageLabel";
      this.messageLabel.Size = new Size(494, 238);
      this.messageLabel.TabIndex = 0;
      this.messageLabel.Text = "Application is not designed to be used in the portrait orientation mode, turn your device into landscape orientation in order to continue.";
      this.messageLabel.TextAlign = ContentAlignment.MiddleCenter;
      this.AutoScaleDimensions = new SizeF(6f, 13f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.BackColor = Color.Black;
      this.ClientSize = new Size(512, 256);
      this.ControlBox = false;
      this.Controls.Add((Control) this.messageLabel);
      this.ForeColor = SystemColors.ControlText;
      this.FormBorderStyle = FormBorderStyle.None;
      this.Name = nameof (OrientationLockForm);
      this.Opacity = 0.8;
      this.StartPosition = FormStartPosition.CenterScreen;
      this.Text = nameof (OrientationLockForm);
      this.TopMost = true;
      this.WindowState = FormWindowState.Maximized;
      this.ResumeLayout(false);
    }
  }
}
