

using SebWindowsClient.DiagnosticsUtils;
using SebWindowsClient.XULRunnerCommunication;
using System;
using System.Collections;
using System.Diagnostics;
using System.Management;
using System.Threading;

namespace SebWindowsClient.ProcessUtils
{
  public class SEBNotAllowedProcessController
  {
    public static bool CheckIfAProcessIsRunning(string processname)
    {
      return (uint) Process.GetProcessesByName(processname).Length > 0U;
    }

    public static string getLocalProcessOwner(int pid)
    {
      string str = "";
      using (ManagementObjectCollection.ManagementObjectEnumerator enumerator = new ManagementObjectSearcher(new ObjectQuery("Select * From Win32_Process where Handle='" + (object) pid + "'")).Get().GetEnumerator())
      {
        if (enumerator.MoveNext())
        {
          ManagementObject current = (ManagementObject) enumerator.Current;
          string[] strArray1 = new string[2];
          string methodName = "GetOwner";
          string[] strArray2 = strArray1;
          current.InvokeMethod(methodName, (object[]) strArray2);
          str = strArray1[0].ToString();
        }
      }
      return str;
    }

    public static void CloseProcessByName(string nameToClosse)
    {
      foreach (Process process in Process.GetProcesses())
      {
        if (process.ProcessName == nameToClosse)
        {
          try
          {
            for (int index = 0; index < 5 && !process.HasExited; ++index)
            {
              process.Refresh();
              Console.WriteLine("Physical Memory Usage: " + process.WorkingSet64.ToString());
              Thread.Sleep(2000);
            }
            process.CloseMainWindow();
            process.Close();
          }
          catch (Exception ex)
          {
            Console.WriteLine("The following exception was raised: ");
            Console.WriteLine(ex.Message);
          }
        }
      }
    }

    public static void CloseProcess(Process processToClose)
    {
      try
      {
        if (processToClose == null)
          return;
        Logger.AddInformation("Closing " + processToClose.ProcessName, (object) null, (Exception) null, (string) null);
        if (processToClose.ProcessName.Contains("xulrunner"))
        {
          Logger.AddInformation("Closing XulRunner over Socket", (object) null, (Exception) null, (string) null);
          SEBXULRunnerWebSocketServer.SendAllowCloseToXulRunner();
          Thread.Sleep(500);
        }
        string str = "processHasExitedTrue";
        if (!processToClose.HasExited)
        {
          Logger.AddInformation("Process " + processToClose.ProcessName + " hasnt closed yet, try again", (object) null, (Exception) null, (string) null);
          if (processToClose.MainWindowHandle != IntPtr.Zero)
          {
            str = processToClose.ProcessName;
            Logger.AddError("Send CloseMainWindow to process " + str, (object) null, (Exception) null, (string) null);
            processToClose.CloseMainWindow();
            for (int index = 0; index < 5; ++index)
            {
              processToClose.Refresh();
              if (processToClose != null && !processToClose.HasExited)
              {
                Logger.AddError("Process " + str + " hasn't exited by closing its main window, wait up to one more second and check again.", (object) null, (Exception) null, (string) null);
                processToClose.WaitForExit(1000);
              }
              else
              {
                Logger.AddError("Process " + str + " has exited.", (object) null, (Exception) null, (string) null);
                break;
              }
            }
          }
        }
        processToClose.Refresh();
        if (!processToClose.HasExited)
        {
          Logger.AddError("Send Kill to process " + str, (object) null, (Exception) null, (string) null);
          processToClose.Kill();
          for (int index = 0; index < 10; ++index)
          {
            processToClose.Refresh();
            if (!processToClose.HasExited)
            {
              Logger.AddError("Process " + str + " still hasn't exited, wait up to one more second and check again.", (object) null, (Exception) null, (string) null);
              try
              {
                processToClose.WaitForExit(1000);
              }
              catch (Exception ex)
              {
                Logger.AddError("Unable to processToClose.WaitForExit(1000)", (object) null, ex, (string) null);
              }
            }
            else
            {
              Logger.AddError("Process " + str + " has exited.", (object) null, (Exception) null, (string) null);
              break;
            }
          }
        }
        processToClose.Refresh();
        if (processToClose.HasExited)
          return;
        Logger.AddError("Process " + str + " has not exited after killing it and waiting in total 11 seconds!", (object) null, (Exception) null, (string) null);
      }
      catch (Exception ex)
      {
        Logger.AddError("Error when killing process", (object) null, ex, (string) null);
      }
    }

    public static Hashtable GetProcesses()
    {
      Hashtable hashtable = new Hashtable();
      foreach (Process process in Process.GetProcesses())
        hashtable.Add((object) Convert.ToInt32(process.Id), (object) process.ProcessName);
      return hashtable;
    }

    public static void KillProcessByName(string nameToKill)
    {
      try
      {
        foreach (Process process in Process.GetProcesses())
        {
          if (process.ProcessName == nameToKill)
            process.Kill();
        }
      }
      catch (Exception ex)
      {
        Logger.AddError("Error when killing process", (object) null, ex, (string) null);
      }
    }

    public static void KillProcessById(int idToKill)
    {
      foreach (Process process in Process.GetProcesses())
      {
        if (process.Id == idToKill)
          process.Kill();
      }
    }
  }
}
