                                                                               

using MetroFramework;
using System.Drawing;
using System.Windows.Forms;

namespace SebWindowsClient.ConfigurationUtils
{
  public class SEBMessageBox
  {
    public static DialogResult Show(string messageTitle, string messageText, MessageBoxIcon messageBoxIcon, MessageBoxButtons messageButtons, bool neverShowTouchOptimized = false)
    {
      if (SEBClientInfo.SebWindowsClientForm != null)
        SebWindowsClientMain.SEBToForeground();
      DialogResult dialogResult;
      if (!neverShowTouchOptimized && (bool) SEBClientInfo.getSebSetting("touchOptimized")["touchOptimized"])
      {
        Form form = new Form();
        form.TopMost = true;
        int num1 = 0;
        form.Top = num1;
        int num2 = 0;
        form.Left = num2;
        Rectangle bounds = Screen.PrimaryScreen.Bounds;
        int width = bounds.Width;
        form.Width = width;
        bounds = Screen.PrimaryScreen.Bounds;
        int height1 = bounds.Height;
        form.Height = height1;
        string message = messageText;
        string title = messageTitle;
        int num3 = (int) messageButtons;
        int num4 = (int) messageBoxIcon;
        int height2 = 211;
        dialogResult = MetroMessageBox.Show((IWin32Window) form, message, title, (MessageBoxButtons) num3, (MessageBoxIcon) num4, height2);
      }
      else
      {
        Form form = new Form();
        form.TopMost = true;
        string text = messageText;
        string caption = messageTitle;
        int num1 = (int) messageButtons;
        int num2 = (int) messageBoxIcon;
        dialogResult = MessageBox.Show((IWin32Window) form, text, caption, (MessageBoxButtons) num1, (MessageBoxIcon) num2);
      }
      return dialogResult;
    }
  }
}
