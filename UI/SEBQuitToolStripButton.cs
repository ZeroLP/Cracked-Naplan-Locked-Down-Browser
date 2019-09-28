

using SebWindowsClient.Properties;
using System.Drawing;
using System.Windows.Forms;

namespace SebWindowsClient.UI
{
  public class SEBQuitToolStripButton : SEBToolStripButton
  {
    public SEBQuitToolStripButton()
    {
      this.ToolTipText = SEBUIStrings.confirmQuitting;
      this.Alignment = ToolStripItemAlignment.Right;
      this.Image = (Image) Resources.ResourceManager.GetObject("quit");
    }
  }
}
