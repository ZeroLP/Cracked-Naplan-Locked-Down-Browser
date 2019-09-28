

using System;
using System.Windows.Forms;

namespace SebWindowsClient
{
  public class Worker
  {
    private volatile bool _shouldStop;
    public volatile string dialogTitle;
    public volatile string dialogText;
    public volatile string dialogResultText;
    public volatile string fileNameExecutable;
    public volatile string fileNameFullPath;

    public void ShowPasswordDialogInThread()
    {
      this.dialogResultText = SebPasswordDialogForm.ShowPasswordDialogForm(this.dialogTitle, this.dialogText);
    }

    public void ShowFileDialogInThread()
    {
      OpenFileDialog openFileDialog = new OpenFileDialog();
      openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
      openFileDialog.FileName = this.fileNameExecutable;
      openFileDialog.Filter = this.fileNameExecutable + " | " + this.fileNameExecutable;
      openFileDialog.CheckFileExists = true;
      openFileDialog.CheckPathExists = true;
      openFileDialog.DefaultExt = "exe";
      openFileDialog.Title = SEBUIStrings.locatePermittedApplication;
      DialogResult dialogResult = openFileDialog.ShowDialog();
      this.fileNameFullPath = (string) null;
      if (!dialogResult.Equals((object) DialogResult.OK) || !openFileDialog.FileName.EndsWith(this.fileNameExecutable))
        return;
      this.fileNameFullPath = openFileDialog.FileName;
    }

    public void RequestStop()
    {
      this._shouldStop = true;
    }
  }
}
