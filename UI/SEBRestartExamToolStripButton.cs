

using SebWindowsClient.ConfigurationUtils;
using SebWindowsClient.CryptographyUtils;
using SebWindowsClient.Properties;
using SebWindowsClient.XULRunnerCommunication;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace SebWindowsClient.UI
{
  public class SEBRestartExamToolStripButton : SEBToolStripButton
  {
    public SEBRestartExamToolStripButton()
    {
      string examDefaultTitle = (string) SEBClientInfo.getSebSetting("restartExamText")["restartExamText"];
      if (string.IsNullOrEmpty(examDefaultTitle))
        examDefaultTitle = SEBUIStrings.restartExamDefaultTitle;
      this.ToolTipText = examDefaultTitle;
      this.Image = (Image) Resources.ResourceManager.GetObject("restartExam");
      this.Alignment = ToolStripItemAlignment.Right;
    }

    protected override void OnClick(EventArgs e)
    {
      string examDefaultTitle = (string) SEBClientInfo.getSebSetting("restartExamText")["restartExamText"];
      if (string.IsNullOrEmpty(examDefaultTitle))
        examDefaultTitle = SEBUIStrings.restartExamDefaultTitle;
      string strA = (string) SEBClientInfo.getSebSetting("hashedQuitPassword")["hashedQuitPassword"];
      if ((bool) SEBClientInfo.getSebSetting("restartExamPasswordProtected")["restartExamPasswordProtected"] && !string.IsNullOrWhiteSpace(strA))
      {
        string input = SebPasswordDialogForm.ShowPasswordDialogForm(examDefaultTitle, SEBUIStrings.restartExamEnterPassword);
        if (input == null)
          return;
        string passwordHash = SEBProtectionController.ComputePasswordHash(input);
        if (string.IsNullOrWhiteSpace(input) || string.Compare(strA, passwordHash, StringComparison.OrdinalIgnoreCase) != 0)
        {
          int num = (int) SEBMessageBox.Show(examDefaultTitle, SEBUIStrings.wrongQuitRestartPasswordText, MessageBoxIcon.Hand, MessageBoxButtons.OK, false);
        }
        else
          SEBXULRunnerWebSocketServer.SendRestartExam();
      }
      else
      {
        if (SEBMessageBox.Show(examDefaultTitle, SEBUIStrings.restartExamConfirm, MessageBoxIcon.Question, MessageBoxButtons.YesNo, false) != DialogResult.Yes)
          return;
        SEBXULRunnerWebSocketServer.SendRestartExam();
      }
    }
  }
}
