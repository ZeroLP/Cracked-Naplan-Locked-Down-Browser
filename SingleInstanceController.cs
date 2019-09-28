
using Microsoft.VisualBasic.ApplicationServices;
using SebWindowsClient.ConfigurationUtils;
using SebWindowsClient.DiagnosticsUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace SebWindowsClient
{
  public class SingleInstanceController : WindowsFormsApplicationBase
  {
    public SingleInstanceController()
    {
      this.IsSingleInstance = true;
      this.StartupNextInstance += new StartupNextInstanceEventHandler(this.this_StartupNextInstance);
    }

    private void this_StartupNextInstance(object sender, StartupNextInstanceEventArgs e)
    {
      SebWindowsClientForm mainForm = this.MainForm as SebWindowsClientForm;
      if (e.CommandLine.Count<string>() <= 1)
        return;
      Logger.AddInformation("StartupNextInstanceEventArgs: " + string.Join(", ", (IEnumerable<string>) e.CommandLine), (object) null, (Exception) null, (string) null);
      if (mainForm.LoadFile(e.CommandLine[1]))
        return;
      Logger.AddError("LoadFile() from StartupNextInstanceEvent failed!", (object) null, (Exception) null, (string) null);
    }

    protected override void OnCreateMainForm()
    {
      this.MainForm = (Form) SEBClientInfo.SebWindowsClientForm;
      if (((IEnumerable<string>) Environment.GetCommandLineArgs()).Count<string>() != 1)
        return;
      new Thread(new ThreadStart(SebWindowsClientMain.StartSplash)).Start();
      try
      {
        SebWindowsClientMain.InitSEBDesktop();
        SebWindowsClientMain.SetProcessLandscapePreference();
      }
      catch (Exception ex)
      {
        Logger.AddError("Unable to InitSEBDesktop", (object) null, ex, (string) null);
      }
      if (!SEBClientInfo.SebWindowsClientForm.OpenSEBForm())
        Logger.AddError("Unable to OpenSEBForm", (object) null, (Exception) null, (string) null);
      SebWindowsClientMain.CloseSplash();
    }

    public void SetMainForm(Form newMainForm)
    {
      this.MainForm = newMainForm;
    }
  }
}
