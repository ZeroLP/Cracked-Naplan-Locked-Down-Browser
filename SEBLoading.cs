
using SebWindowsClient.ConfigurationUtils;
using SebWindowsClient.DesktopUtils;
using SebWindowsClient.DiagnosticsUtils;
using SebWindowsClient.Properties;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SebWindowsClient
{
  public class SEBLoading : Form
  {
    private static SEBLoading loading;
    private IContainer components;
    private PictureBox pictureBox2;
    private Label label1;

    public SEBLoading()
    {
      this.InitializeComponent();
    }

    public void KillMe(object o, EventArgs e)
    {
      this.Close();
    }

    public static void StartLoading()
    {
      if ((bool) SEBClientInfo.getSebSetting("createNewDesktop")["createNewDesktop"] || SEBClientInfo.CreateNewDesktopOldValue)
        SEBDesktopController.SetCurrent(SEBClientInfo.SEBNewlDesktop);
      else
        SEBDesktopController.SetCurrent(SEBClientInfo.OriginalDesktop);
      SEBLoading.loading = new SEBLoading();
      Application.Run((Form) SEBLoading.loading);
    }

    public static void CloseLoading()
    {
      if (SEBLoading.loading == null)
        return;
      try
      {
        Logger.AddInformation("shutting down loading screen", (object) null, (Exception) null, (string) null);
        if (SEBLoading.loading.InvokeRequired)
          SEBLoading.loading.Invoke((Delegate) new EventHandler(SEBLoading.loading.KillMe));
        else
          SEBLoading.loading.Close();
        Logger.AddInformation("loading screen shut down", (object) null, (Exception) null, (string) null);
      }
      catch (Exception ex)
      {
      }
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      this.pictureBox2 = new PictureBox();
      this.label1 = new Label();
      ((ISupportInitialize) this.pictureBox2).BeginInit();
      this.SuspendLayout();
      this.pictureBox2.BackColor = Color.Transparent;
      this.pictureBox2.BackgroundImageLayout = ImageLayout.None;
      this.pictureBox2.Image = (Image) Resources.loading;
      this.pictureBox2.Location = new Point(12, 12);
      this.pictureBox2.Name = "pictureBox2";
      this.pictureBox2.Size = new Size(58, 57);
      this.pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
      this.pictureBox2.TabIndex = 2;
      this.pictureBox2.TabStop = false;
      this.label1.AutoSize = true;
      this.label1.BackColor = Color.Transparent;
      this.label1.Font = new Font("Microsoft Sans Serif", 9.75f);
      this.label1.Location = new Point(76, 53);
      this.label1.Name = "label1";
      this.label1.Size = new Size(86, 16);
      this.label1.TabIndex = 5;
      this.label1.Text = "Please wait...";
      this.AutoScaleDimensions = new SizeF(6f, 13f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.BackColor = Color.White;
      this.BackgroundImageLayout = ImageLayout.Stretch;
      this.ClientSize = new Size(329, 83);
      this.ControlBox = false;
      this.Controls.Add((Control) this.label1);
      this.Controls.Add((Control) this.pictureBox2);
      this.FormBorderStyle = FormBorderStyle.FixedSingle;
      this.Margin = new Padding(2, 2, 2, 2);
      this.MinimizeBox = false;
      this.Name = nameof (SEBLoading);
      this.ShowInTaskbar = false;
      this.StartPosition = FormStartPosition.CenterScreen;
      this.TopMost = true;
      ((ISupportInitialize) this.pictureBox2).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();
    }
  }
}
