
using SebWindowsClient.DiagnosticsUtils;
using SebWindowsClient.ProcessUtils;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SebWindowsClient.UI
{
  public class SEBInputLanguageToolStripButton : SEBToolStripButton
  {
    private Timer timer;
    private int currentIndex;
    private IntPtr[] languages;
    private const int WM_INPUTLANGCHANGEREQUEST = 80;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool PostMessage(IntPtr hWnd, [MarshalAs(UnmanagedType.U4)] uint Msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern IntPtr LoadKeyboardLayout(string pwszKLID, uint Flags);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr id);

    [DllImport("user32.dll")]
    private static extern uint GetKeyboardLayout(uint idThread);

    public SEBInputLanguageToolStripButton()
    {
      this.Alignment = ToolStripItemAlignment.Right;
      this.ForeColor = Color.Black;
      this.Font = new Font("Arial", (float) this.FontSize, FontStyle.Bold);
      try
      {
        this.currentIndex = InputLanguage.InstalledInputLanguages.IndexOf(InputLanguage.CurrentInputLanguage);
        if (InputLanguage.InstalledInputLanguages.Count < 2)
          throw new NotSupportedException("There is only one keyboard layout available");
        SEBWindowHandler.ForegroundWatchDog.OnForegroundWindowChanged += (ForegroundWatchDog.ForegroundWindowChangedEventHandler) (handle => this.SetKeyboardLayoutAccordingToIndex());
        this.languages = new IntPtr[InputLanguage.InstalledInputLanguages.Count];
        for (int index = 0; index < InputLanguage.InstalledInputLanguages.Count; ++index)
          this.languages[index] = SEBInputLanguageToolStripButton.LoadKeyboardLayout(InputLanguage.InstalledInputLanguages[index].Culture.KeyboardLayoutId.ToString("X8"), 1U);
        this.timer = new Timer();
        this.timer.Tick += (EventHandler) ((x, y) => this.UpdateDisplayText());
        this.timer.Interval = 1000;
        this.timer.Start();
      }
      catch (Exception ex)
      {
        this.Enabled = false;
        this.UpdateDisplayText();
      }
    }

    private void SetKeyboardLayoutAccordingToIndex()
    {
      try
      {
        InputLanguage.CurrentInputLanguage = InputLanguage.InstalledInputLanguages[this.currentIndex];
        if ((long) this.languages[this.currentIndex].ToInt32() != (long) SEBInputLanguageToolStripButton.GetKeyboardLayout(SEBInputLanguageToolStripButton.GetWindowThreadProcessId(SEBWindowHandler.GetForegroundWindow(), IntPtr.Zero)))
          SEBInputLanguageToolStripButton.PostMessage(SEBWindowHandler.GetForegroundWindow(), 80U, IntPtr.Zero, this.languages[this.currentIndex]);
      }
      catch (Exception ex)
      {
        Logger.AddError("Could not change InputLanguage", (object) this, ex, (string) null);
      }
      this.UpdateDisplayText();
    }

    private void UpdateDisplayText()
    {
      try
      {
        this.Text = InputLanguage.CurrentInputLanguage.Culture.ThreeLetterISOLanguageName.ToUpper();
        this.ToolTipText = string.Format(SEBUIStrings.KeyboardLayout_CURRENTCULTURE, (object) InputLanguage.CurrentInputLanguage.Culture.DisplayName);
      }
      catch (Exception ex)
      {
        this.Text = "Error";
      }
    }

    protected override void OnMouseHover(EventArgs e)
    {
      if (this.Parent != null)
        this.Parent.Focus();
      base.OnMouseHover(e);
    }

    protected override void OnClick(EventArgs e)
    {
      try
      {
        this.currentIndex = this.currentIndex + 1;
        this.currentIndex = this.currentIndex % InputLanguage.InstalledInputLanguages.Count;
      }
      catch (Exception ex)
      {
        Logger.AddError("Could not change InputLanguage", (object) this, ex, (string) null);
      }
      this.SetKeyboardLayoutAccordingToIndex();
    }
  }
}
