

using SebWindowsClient.ConfigurationUtils;
using SebWindowsClient.CryptographyUtils;
using SebWindowsClient.ProcessUtils;
using SebWindowsClient.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SebWindowsClient
{
  public class SebCloseDialogForm : Form
  {
    private IContainer components;
    private Button btnOk;
    private Button btnCancel;
    private Label lblQuitPassword;
    public MaskedTextBox txtQuitPassword;

    public SebCloseDialogForm()
    {
      this.InitializeComponent();
      if ((bool) SEBClientInfo.getSebSetting("touchOptimized")["touchOptimized"])
        this.InitializeForTouch();
      else
        this.InitializeForNonTouch();
    }

    public void InitializeForTouch()
    {
      this.Font = new Font(FontFamily.GenericSansSerif, 12f);
      this.lblQuitPassword.Left = Screen.PrimaryScreen.Bounds.Width / 2 - this.lblQuitPassword.Width / 2;
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
      this.txtQuitPassword.Width = 400;
      this.txtQuitPassword.Left = Screen.PrimaryScreen.Bounds.Width / 2 - this.txtQuitPassword.Width / 2;
      this.txtQuitPassword.Height = 30;
    }

    public void InitializeForNonTouch()
    {
      this.Font = Control.DefaultFont;
      this.lblQuitPassword.Left = (int) (12.0 * (double) SEBClientInfo.scaleFactor);
      this.FormBorderStyle = FormBorderStyle.FixedDialog;
      this.Width = (int) (365.0 * (double) SEBClientInfo.scaleFactor);
      this.Height = (int) (155.0 * (double) SEBClientInfo.scaleFactor);
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
      this.txtQuitPassword.Width = (int) (325.0 * (double) SEBClientInfo.scaleFactor);
      this.txtQuitPassword.Left = (int) (12.0 * (double) SEBClientInfo.scaleFactor);
      this.txtQuitPassword.Height = (int) (20.0 * (double) SEBClientInfo.scaleFactor);
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
      this.txtQuitPassword.Text = "";
      this.Visible = false;
      if (!(bool) SEBSettings.valueForDictionaryKey(SEBSettings.settingsCurrent, "touchOptimized"))
        return;
      KeyValuePair<IntPtr, string> keyValuePair = SEBWindowHandler.GetOpenWindows().FirstOrDefault<KeyValuePair<IntPtr, string>>();
      if (keyValuePair.Value == null)
        return;
      keyValuePair.Key.AdaptWindowToWorkingArea(new int?());
    }

    private void btnOk_Click(object sender, EventArgs e)
    {
      if (string.Compare((string) SEBClientInfo.getSebSetting("hashedQuitPassword")["hashedQuitPassword"], SEBProtectionController.ComputePasswordHash(this.txtQuitPassword.Text), StringComparison.OrdinalIgnoreCase) != 0)
      {
        this.Hide();
        int num = (int) SEBMessageBox.Show(SEBUIStrings.quittingFailed, SEBUIStrings.quittingFailedReason, MessageBoxIcon.Hand, MessageBoxButtons.OK, false);
        this.txtQuitPassword.Text = "";
        this.Visible = false;
      }
      else
      {
        this.Visible = false;
        SEBClientInfo.SebWindowsClientForm.ExitApplication(true);
      }
    }

    private void txtQuitPassword_Enter(object sender, EventArgs e)
    {
      TapTipHandler.ShowKeyboard(false);
    }

    private void txtQuitPassword_Leave(object sender, EventArgs e)
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
      ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof (SebCloseDialogForm));
      this.txtQuitPassword = new MaskedTextBox();
      this.btnOk = new Button();
      this.btnCancel = new Button();
      this.lblQuitPassword = new Label();
      this.SuspendLayout();
      MaskedTextBox txtQuitPassword = this.txtQuitPassword;
      string objectName1 = "txtQuitPassword";
      componentResourceManager.ApplyResources((object) txtQuitPassword, objectName1);
      this.txtQuitPassword.Name = "txtQuitPassword";
      this.txtQuitPassword.PasswordChar = '●';
      this.txtQuitPassword.Enter += new EventHandler(this.txtQuitPassword_Enter);
      this.txtQuitPassword.Leave += new EventHandler(this.txtQuitPassword_Leave);
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
      Label lblQuitPassword = this.lblQuitPassword;
      string objectName4 = "lblQuitPassword";
      componentResourceManager.ApplyResources((object) lblQuitPassword, objectName4);
      this.lblQuitPassword.Name = "lblQuitPassword";
      this.AcceptButton = (IButtonControl) this.btnOk;
      string objectName5 = "$this";
      componentResourceManager.ApplyResources((object) this, objectName5);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.CancelButton = (IButtonControl) this.btnCancel;
      this.ControlBox = false;
      this.Controls.Add((Control) this.lblQuitPassword);
      this.Controls.Add((Control) this.btnCancel);
      this.Controls.Add((Control) this.btnOk);
      this.Controls.Add((Control) this.txtQuitPassword);
      this.FormBorderStyle = FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = nameof (SebCloseDialogForm);
      this.ResumeLayout(false);
      this.PerformLayout();
    }
  }
}
