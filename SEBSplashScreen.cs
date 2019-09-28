

using SebWindowsClient.ConfigurationUtils;
using SebWindowsClient.DesktopUtils;
using SebWindowsClient.Properties;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace SebWindowsClient
{
  public class SEBSplashScreen : Form
  {
    private static SEBSplashScreen splash;
    private IContainer components;
    private Label lblLoading;
    private TextBox TxtVersion;
    private TextBox textBox2;
    private PictureBox pictureBox1;

    public SEBSplashScreen()
    {
      this.InitializeComponent();
      AssemblyInformationalVersionAttribute customAttribute = (AssemblyInformationalVersionAttribute) Assembly.GetEntryAssembly().GetCustomAttribute(typeof (AssemblyInformationalVersionAttribute));
      this.textBox2.Text = string.Format(Resources.SplashScreenTextTpl, customAttribute != null ? (object) customAttribute.InformationalVersion : (object) (string) null);
      float dpiX;
      using (Graphics graphics = this.CreateGraphics())
        dpiX = graphics.DpiX;
      double num = (double) dpiX / 96.0;
      int width = this.pictureBox1.Width;
      int height = this.pictureBox1.Height;
      this.Click += new EventHandler(this.KillMe);
      this.pictureBox1.Click += new EventHandler(this.KillMe);
      this.TxtVersion.Click += new EventHandler(this.KillMe);
      this.lblLoading.Click += new EventHandler(this.KillMe);
      Timer timer = new Timer();
      timer.Interval = 200;
      EventHandler eventHandler = (EventHandler) ((sender, args) => this.Progress());
      timer.Tick += eventHandler;
      timer.Start();
    }

    private void Progress()
    {
      string text = this.lblLoading.Text;
      if (!(text == "Loading"))
      {
        if (!(text == "Loading ."))
        {
          if (text == "Loading ..")
            this.lblLoading.Text = "Loading ...";
          else
            this.lblLoading.Text = "Loading";
        }
        else
          this.lblLoading.Text = "Loading ..";
      }
      else
        this.lblLoading.Text = "Loading .";
    }

    public void KillMe(object o, EventArgs e)
    {
      this.Close();
    }

    public static void StartSplash()
    {
      if (SEBClientInfo.SEBNewlDesktop != null && (bool) SEBClientInfo.getSebSetting("createNewDesktop")["createNewDesktop"])
        SEBDesktopController.SetCurrent(SEBClientInfo.SEBNewlDesktop);
      else
        SEBDesktopController.SetCurrent(SEBClientInfo.OriginalDesktop);
      SEBSplashScreen.splash = new SEBSplashScreen();
      Application.Run((Form) SEBSplashScreen.splash);
    }

    public static void CloseSplash()
    {
      if (SEBSplashScreen.splash == null)
        return;
      try
      {
        SEBSplashScreen.splash.Invoke((Delegate) new EventHandler(SEBSplashScreen.splash.KillMe));
        SEBSplashScreen.splash.Dispose();
        SEBSplashScreen.splash = (SEBSplashScreen) null;
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
      ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof (SEBSplashScreen));
      this.lblLoading = new Label();
      this.TxtVersion = new TextBox();
      this.textBox2 = new TextBox();
      this.pictureBox1 = new PictureBox();
      ((ISupportInitialize) this.pictureBox1).BeginInit();
      this.SuspendLayout();
      this.lblLoading.BackColor = Color.Transparent;
      this.lblLoading.Dock = DockStyle.Bottom;
      this.lblLoading.Font = new Font("Microsoft Sans Serif", 9.75f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0);
      this.lblLoading.Location = new Point(0, 121);
      this.lblLoading.Margin = new Padding(2, 0, 2, 0);
      this.lblLoading.Name = "lblLoading";
      this.lblLoading.Padding = new Padding(12, 0, 0, 5);
      this.lblLoading.Size = new Size(594, 22);
      this.lblLoading.TabIndex = 0;
      this.lblLoading.Text = "Loading ...";
      this.lblLoading.TextAlign = ContentAlignment.MiddleLeft;
      this.TxtVersion.AccessibleRole = AccessibleRole.None;
      this.TxtVersion.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
      this.TxtVersion.BackColor = Color.White;
      this.TxtVersion.BorderStyle = BorderStyle.None;
      this.TxtVersion.CausesValidation = false;
      this.TxtVersion.Enabled = false;
      this.TxtVersion.Font = new Font("Microsoft Sans Serif", 6.75f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0);
      this.TxtVersion.Location = new Point(301, 5);
      this.TxtVersion.Multiline = true;
      this.TxtVersion.Name = "TxtVersion";
      this.TxtVersion.Size = new Size(293, 18);
      this.TxtVersion.TabIndex = 2;
      this.TxtVersion.TextAlign = HorizontalAlignment.Right;
      this.textBox2.AccessibleRole = AccessibleRole.None;
      this.textBox2.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
      this.textBox2.BackColor = Color.White;
      this.textBox2.BorderStyle = BorderStyle.None;
      this.textBox2.CausesValidation = false;
      this.textBox2.Enabled = false;
      this.textBox2.Font = new Font("Microsoft Sans Serif", 6.75f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0);
      this.textBox2.Location = new Point(3, 5);
      this.textBox2.Margin = new Padding(3, 3, 13, 3);
      this.textBox2.Multiline = true;
      this.textBox2.Name = "textBox2";
      this.textBox2.Size = new Size(591, 113);
      this.textBox2.TabIndex = 3;
      this.pictureBox1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
      this.pictureBox1.Image = (Image) componentResourceManager.GetObject("pictureBox1.Image");
      this.pictureBox1.Location = new Point(0, 243);
      this.pictureBox1.Name = "pictureBox1";
      this.pictureBox1.Size = new Size(594, 0);
      this.pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
      this.pictureBox1.TabIndex = 1;
      this.pictureBox1.TabStop = false;
      this.pictureBox1.Visible = false;
      this.AutoScaleDimensions = new SizeF(6f, 13f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.BackColor = Color.White;
      this.BackgroundImageLayout = ImageLayout.None;
      this.ClientSize = new Size(594, 143);
      this.ControlBox = false;
      this.Controls.Add((Control) this.TxtVersion);
      this.Controls.Add((Control) this.lblLoading);
      this.Controls.Add((Control) this.pictureBox1);
      this.Controls.Add((Control) this.textBox2);
      this.FormBorderStyle = FormBorderStyle.FixedDialog;
      this.Margin = new Padding(2);
      this.MinimizeBox = false;
      this.Name = nameof (SEBSplashScreen);
      this.ShowInTaskbar = false;
      this.StartPosition = FormStartPosition.CenterScreen;
      ((ISupportInitialize) this.pictureBox1).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();
    }
  }
}
