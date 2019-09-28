
using SebWindowsClient.ConfigurationUtils;
using SebWindowsClient.ProcessUtils;
using SebWindowsClient.Properties;
using SebWindowsClient.XULRunnerCommunication;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SebWindowsClient.UI
{
  public class SEBReloadBrowserToolStripButton : SEBToolStripButton
  {
    public SEBReloadBrowserToolStripButton()
    {
      this.ToolTipText = SEBUIStrings.reloadPage;
      this.Image = (Image) Resources.ResourceManager.GetObject("reload");
      this.BackColor = Color.White;
      this.Alignment = ToolStripItemAlignment.Right;
    }

    protected override void OnClick(EventArgs e)
    {
      try
      {
        SEBWindowHandler.BringWindowToTop(SEBWindowHandler.GetOpenWindows().First<KeyValuePair<IntPtr, string>>((Func<KeyValuePair<IntPtr, string>, bool>) (w => w.Key.GetProcess().GetExecutableName().Contains("xul"))).Key);
        if ((bool) SEBSettings.settingsCurrent["touchOptimized"])
        {
          if ((bool) SEBSettings.settingsCurrent["showReloadWarning"])
          {
            if (SEBMessageBox.Show(SEBUIStrings.reloadPage, SEBUIStrings.reloadPageMessage, MessageBoxIcon.Exclamation, MessageBoxButtons.YesNo, false) != DialogResult.Yes)
              return;
            SEBXULRunnerWebSocketServer.SendReloadPage();
          }
          else
            SEBXULRunnerWebSocketServer.SendReloadPage();
        }
        else
          SEBXULRunnerWebSocketServer.SendReloadPage();
      }
      catch
      {
      }
    }
  }
}
