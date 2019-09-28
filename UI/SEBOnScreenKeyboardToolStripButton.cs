
using SebWindowsClient.Properties;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace SebWindowsClient.UI
{
  public class SEBOnScreenKeyboardToolStripButton : SEBToolStripButton
  {
    public SEBOnScreenKeyboardToolStripButton()
    {
      this.InitializeComponent();
      this.Alignment = ToolStripItemAlignment.Right;
    }

    protected override void OnClick(EventArgs e)
    {
      bool firstOpen = TapTipHandler.FirstOpen;
      if (TapTipHandler.IsKeyboardVisible())
      {
        TapTipHandler.HideKeyboard();
      }
      else
      {
        TapTipHandler.FirstOpen = firstOpen;
        TapTipHandler.ShowKeyboard(true);
      }
    }

    private void InitializeComponent()
    {
      this.ToolTipText = SEBUIStrings.toolTipOnScreenKeyboard;
      this.Image = (Image) Resources.ResourceManager.GetObject("keyboard");
    }
  }
}
