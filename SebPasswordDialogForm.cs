

using SebWindowsClient.ConfigurationUtils;
using SebWindowsClient.ProcessUtils;
using SebWindowsClient.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SebWindowsClient
{
  public class SebPasswordDialogForm : Form
  {
    private IContainer components;
    private Button btnOk;
    private Button btnCancel;
    private Label lblSEBPassword;
    public MaskedTextBox txtSEBPassword;

    [DllImport("User32.dll")]
    public static extern IntPtr SetForegroundWindow(IntPtr hWnd);

    public static string ShowPasswordDialogForm(string title, string passwordRequestText)
    {
      using (SebPasswordDialogForm passwordDialogForm = new SebPasswordDialogForm())
      {
        SebPasswordDialogForm.SetForegroundWindow(passwordDialogForm.Handle);
        passwordDialogForm.TopMost = true;
        passwordDialogForm.Text = title;
        passwordDialogForm.LabelText = passwordRequestText;
        passwordDialogForm.txtSEBPassword.Focus();
        if (passwordDialogForm.ShowDialog() != DialogResult.OK)
          return (string) null;
        string text = passwordDialogForm.txtSEBPassword.Text;
        passwordDialogForm.txtSEBPassword.Text = "";
        return text;
      }
    }

    public SebPasswordDialogForm()
    {
      this.InitializeComponent();
      try
      {
        if ((bool) SEBClientInfo.getSebSetting("touchOptimized")["touchOptimized"])
          this.InitializeForTouch();
        else
          this.InitializeForNonTouch();
      }
      catch
      {
      }
    }

    public void InitializeForTouch()
    {
      this.Font = new Font(FontFamily.GenericSansSerif, 12f);
      IntPtr handle = this.Handle;
      this.FormBorderStyle = FormBorderStyle.None;
      this.Top = 0;
      this.Left = 0;
      this.Width = Screen.PrimaryScreen.Bounds.Width;
      this.Height = Screen.PrimaryScreen.Bounds.Height;
      this.btnCancel.BackColor = Color.Red;
      this.btnCancel.FlatStyle = FlatStyle.Flat;
      this.btnCancel.Height = 35;
      this.btnCancel.Width = 120;
      this.btnCancel.Left = Screen.PrimaryScreen.Bounds.Width / 2 - this.btnCancel.Width / 2 + 100;
      this.btnOk.BackColor = Color.Green;
      this.btnOk.FlatStyle = FlatStyle.Flat;
      this.btnOk.Height = 35;
      this.btnOk.Width = 120;
      this.btnOk.Left = Screen.PrimaryScreen.Bounds.Width / 2 - this.btnOk.Width / 2 - 100;
      this.txtSEBPassword.Width = 400;
      this.txtSEBPassword.Left = Screen.PrimaryScreen.Bounds.Width / 2 - this.txtSEBPassword.Width / 2;
      this.txtSEBPassword.Height = 30;
    }

    public void InitializeForNonTouch()
    {
      this.Font = Control.DefaultFont;
      this.lblSEBPassword.Left = (int) (12.0 * (double) SEBClientInfo.scaleFactor);
      this.FormBorderStyle = FormBorderStyle.FixedDialog;
      this.Width = (int) (365.0 * (double) SEBClientInfo.scaleFactor);
      this.Height = (int) (175.0 * (double) SEBClientInfo.scaleFactor);
      this.Top = Screen.PrimaryScreen.Bounds.Height / 2 - this.Height / 2;
      this.Left = Screen.PrimaryScreen.Bounds.Width / 2 - this.Width / 2;
      this.btnCancel.BackColor = SystemColors.Control;
      this.btnCancel.FlatStyle = FlatStyle.Standard;
      this.btnCancel.Height = (int) (23.0 * (double) SEBClientInfo.scaleFactor);
      this.btnCancel.Width = (int) (75.0 * (double) SEBClientInfo.scaleFactor);
      this.btnCancel.Left = (int) (180.0 * (double) SEBClientInfo.scaleFactor);
      this.btnOk.BackColor = SystemColors.Control;
      this.btnOk.FlatStyle = FlatStyle.Standard;
      this.btnOk.Height = (int) (23.0 * (double) SEBClientInfo.scaleFactor);
      this.btnOk.Width = (int) (75.0 * (double) SEBClientInfo.scaleFactor);
      this.btnOk.Left = (int) (94.0 * (double) SEBClientInfo.scaleFactor);
      this.txtSEBPassword.Width = (int) (325.0 * (double) SEBClientInfo.scaleFactor);
      this.txtSEBPassword.Left = (int) (12.0 * (double) SEBClientInfo.scaleFactor);
      this.txtSEBPassword.Height = (int) (20.0 * (double) SEBClientInfo.scaleFactor);
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
      this.txtSEBPassword.Text = "";
      this.ResizeTopOpenWindow();
    }

    private void btnOk_Click(object sender, EventArgs e)
    {
      this.Visible = false;
      this.ResizeTopOpenWindow();
    }

    private void ResizeTopOpenWindow()
    {
      try
      {
        if (!(bool) SEBSettings.valueForDictionaryKey(SEBSettings.settingsCurrent, "touchOptimized"))
          return;
        KeyValuePair<IntPtr, string> keyValuePair = SEBWindowHandler.GetOpenWindows().FirstOrDefault<KeyValuePair<IntPtr, string>>();
        if (keyValuePair.Value == null)
          return;
        keyValuePair.Key.AdaptWindowToWorkingArea(new int?());
      }
      catch
      {
      }
    }

    public string LabelText
    {
      get
      {
        return this.lblSEBPassword.Text;
      }
      set
      {
        this.lblSEBPassword.Text = value;
        try
        {
          if (!(bool) SEBClientInfo.getSebSetting("touchOptimized")["touchOptimized"])
            return;
          this.lblSEBPassword.Left = Screen.PrimaryScreen.Bounds.Width / 2 - this.lblSEBPassword.Width / 2;
        }
        catch (Exception ex)
        {
        }
      }
    }

    private void txtSEBPassword_Enter(object sender, EventArgs e)
    {
      TapTipHandler.ShowKeyboard(false);
    }

    private void txtSEBPassword_Leave(object sender, EventArgs e)
    {
      TapTipHandler.HideKeyboard();
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof (SebPasswordDialogForm));
      this.txtSEBPassword = new MaskedTextBox();
      this.btnOk = new Button();
      this.btnCancel = new Button();
      this.lblSEBPassword = new Label();
      this.SuspendLayout();
      MaskedTextBox txtSebPassword = this.txtSEBPassword;
      string objectName1 = "txtSEBPassword";
      componentResourceManager.ApplyResources((object) txtSebPassword, objectName1);
      this.txtSEBPassword.Name = "txtSEBPassword";
      this.txtSEBPassword.PasswordChar = '●';
      this.txtSEBPassword.Enter += new EventHandler(this.txtSEBPassword_Enter);
      this.txtSEBPassword.Leave += new EventHandler(this.txtSEBPassword_Leave);
      Button btnOk = this.btnOk;
      string objectName2 = "btnOk";
      componentResourceManager.ApplyResources((object) btnOk, objectName2);
      this.btnOk.DialogResult = DialogResult.OK;
      this.btnOk.Name = "btnOk";
      this.btnOk.UseVisualStyleBackColor = true;
      this.btnOk.Click += new EventHandler(this.btnOk_Click);
      Button btnCancel = this.btnCancel;
      string objectName3 = "btnCancel";
      componentResourceManager.ApplyResources((object) btnCancel, objectName3);
      this.btnCancel.DialogResult = DialogResult.Cancel;
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      this.btnCancel.Click += new EventHandler(this.btnCancel_Click);
      Label lblSebPassword = this.lblSEBPassword;
      string objectName4 = "lblSEBPassword";
      componentResourceManager.ApplyResources((object) lblSebPassword, objectName4);
      this.lblSEBPassword.Name = "lblSEBPassword";
      this.AcceptButton = (IButtonControl) this.btnOk;
      string objectName5 = "$this";
      componentResourceManager.ApplyResources((object) this, objectName5);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.CancelButton = (IButtonControl) this.btnCancel;
      this.ControlBox = false;
      this.Controls.Add((Control) this.lblSEBPassword);
      this.Controls.Add((Control) this.btnCancel);
      this.Controls.Add((Control) this.btnOk);
      this.Controls.Add((Control) this.txtSEBPassword);
      this.FormBorderStyle = FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = nameof (SebPasswordDialogForm);
      this.ResumeLayout(false);
      this.PerformLayout();
    }
  }
}
