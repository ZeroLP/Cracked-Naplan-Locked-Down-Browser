

using SebWindowsClient.ConfigurationUtils;
using SebWindowsClient.DesktopUtils;
using System.Threading;

namespace SebWindowsClient
{
  public class ThreadedDialog
  {
    public static string ShowPasswordDialogForm(string title, string passwordRequestText)
    {
      if (SEBClientInfo.SebWindowsClientForm != null)
        SebWindowsClientMain.SEBToForeground();
      if (SebWindowsClientMain.sessionCreateNewDesktop || SEBSettings.settingsCurrent.Count > 0 && (bool) SEBClientInfo.getSebSetting("touchOptimized")["touchOptimized"])
        return SebPasswordDialogForm.ShowPasswordDialogForm(title, passwordRequestText);
      Worker worker = new Worker();
      Thread thread = new Thread(new ThreadStart(worker.ShowPasswordDialogInThread));
      worker.dialogTitle = title;
      worker.dialogText = passwordRequestText;
      thread.Start();
      do
        ;
      while (!thread.IsAlive);
      thread.Join();
      if (SebWindowsClientMain.sessionCreateNewDesktop)
        SEBDesktopController.Show(SEBClientInfo.SEBNewlDesktop.DesktopName);
      return worker.dialogResultText;
    }

    public static string ShowFileDialogForExecutable(string fileName)
    {
      if (SebWindowsClientMain.sessionCreateNewDesktop)
        SEBDesktopController.Show(SEBClientInfo.OriginalDesktop.DesktopName);
      Worker worker = new Worker();
      Thread thread = new Thread(new ThreadStart(worker.ShowFileDialogInThread));
      thread.SetApartmentState(ApartmentState.STA);
      worker.fileNameExecutable = fileName;
      thread.Start();
      do
        ;
      while (!thread.IsAlive);
      thread.Join();
      if (SebWindowsClientMain.sessionCreateNewDesktop)
        SEBDesktopController.Show(SEBClientInfo.SEBNewlDesktop.DesktopName);
      return worker.fileNameFullPath;
    }
  }
}
