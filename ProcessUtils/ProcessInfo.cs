

using System;
using System.Data;
using System.Management;

namespace SebWindowsClient.ProcessUtils
{
  public class ProcessInfo
  {
    public ProcessInfo.StartedEventHandler Started;
    public ProcessInfo.TerminatedEventHandler Terminated;
    private ManagementEventWatcher watcher;

    public string ProcessName { get; set; }

    public ProcessInfo(string appName)
    {
      this.ProcessName = appName;
      this.watcher = new ManagementEventWatcher("\\\\.\\root\\CIMV2", "SELECT *  FROM __InstanceOperationEvent WITHIN  " + "2" + " WHERE TargetInstance ISA 'Win32_Process'    AND TargetInstance.Name = '" + appName + "'");
      this.watcher.EventArrived += new EventArrivedEventHandler(this.OnEventArrived);
      this.watcher.Start();
    }

    public void Dispose()
    {
      this.watcher.Stop();
      this.watcher.Dispose();
    }

    public static DataTable RunningProcesses()
    {
      ManagementObjectCollection objectCollection = new ManagementObjectSearcher(new ManagementScope("\\\\.\\root\\CIMV2"), (ObjectQuery) new SelectQuery("SELECT Name, ProcessId, Caption, ExecutablePath  FROM Win32_Process")).Get();
      DataTable dataTable = new DataTable();
      dataTable.Columns.Add("Name", Type.GetType("System.String"));
      dataTable.Columns.Add("ProcessId", Type.GetType("System.Int32"));
      dataTable.Columns.Add("Caption", Type.GetType("System.String"));
      dataTable.Columns.Add("Path", Type.GetType("System.String"));
      foreach (ManagementObject managementObject in objectCollection)
      {
        DataRow row = dataTable.NewRow();
        row["Name"] = (object) managementObject["Name"].ToString();
        row["ProcessId"] = (object) Convert.ToInt32(managementObject["ProcessId"]);
        if (managementObject["Caption"] != null)
          row["Caption"] = (object) managementObject["Caption"].ToString();
        if (managementObject["ExecutablePath"] != null)
          row["Path"] = (object) managementObject["ExecutablePath"].ToString();
        dataTable.Rows.Add(row);
      }
      return dataTable;
    }

    private void OnEventArrived(object sender, EventArrivedEventArgs e)
    {
      try
      {
        string className = e.NewEvent.ClassPath.ClassName;
        if (className.CompareTo("__InstanceCreationEvent") == 0)
        {
          if (this.Started == null)
            return;
          this.Started((object) this, (EventArgs) e);
        }
        else
        {
          if (className.CompareTo("__InstanceDeletionEvent") != 0 || this.Terminated == null)
            return;
          this.Terminated((object) this, (EventArgs) e);
        }
      }
      catch (Exception ex)
      {
      }
    }

    public delegate void StartedEventHandler(object sender, EventArgs e);

    public delegate void TerminatedEventHandler(object sender, EventArgs e);
  }
}
