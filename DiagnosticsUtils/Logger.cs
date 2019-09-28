

using SebWindowsClient.ConfigurationUtils;
using System;
using System.IO;

namespace SebWindowsClient.DiagnosticsUtils
{
  public class Logger
  {
    public static void AddError(string message, object eventSource, Exception exception, string details = null)
    {
      Logger.Log(Logger.Severity.Error, message, eventSource, exception, details);
    }

    public static void AddWarning(string message, object eventSource, Exception exception = null, string details = null)
    {
      Logger.Log(Logger.Severity.Warning, message, eventSource, exception, details);
    }

    public static void AddInformation(string message, object eventSource = null, Exception exception = null, string details = null)
    {
      Logger.Log(Logger.Severity.Information, message, eventSource, exception, details);
    }

    public static void InitLogger(string logFileDirectory = null, string logFilePath = null)
    {
      try
      {
        if (string.IsNullOrEmpty(logFileDirectory))
        {
          logFileDirectory = SEBClientInfo.SebClientLogFileDirectory;
          if (string.IsNullOrEmpty(logFileDirectory))
            throw new DirectoryNotFoundException();
        }
        if (!Directory.Exists(logFileDirectory))
          Directory.CreateDirectory(logFileDirectory);
        if (string.IsNullOrEmpty(logFilePath))
        {
          logFilePath = SEBClientInfo.SebClientLogFile;
          if (string.IsNullOrEmpty(logFilePath))
            logFilePath = string.Format("{0}\\{1}", (object) logFileDirectory, (object) "SebClient.log");
        }
        Logger.LogFilePath = logFilePath;
      }
      catch (Exception ex)
      {
        Logger.LogFilePath = string.Format("{0}\\{1}", (object) Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), (object) "SafeExamBrowser");
      }
    }

    private static string LogFilePath { get; set; }

    private static void Log(Logger.Severity severity, string message, object eventSource, Exception exception, string details = null)
    {
      try
      {
        using (StreamWriter streamWriter = new StreamWriter(Logger.LogFilePath, true))
        {
          string str1 = eventSource == null ? "" : string.Format(" ({0})", eventSource);
          string str2 = exception == null ? "" : string.Format("\n\n Exception: {0} - {1}\n{2}", (object) exception, (object) exception.Message, (object) exception.StackTrace);
          string str3 = details == null || exception != null && details == exception.Message ? "" : string.Format("\n\n{0}", (object) details);
          streamWriter.WriteLine("{0} [{1}]: {2}{3}{4}{5}\n", (object) DateTime.Now.ToLocalTime(), (object) severity, (object) message, (object) str1, (object) str2, (object) str3);
        }
      }
      catch
      {
      }
    }

    private enum Severity
    {
      Error,
      Warning,
      Information,
    }
  }
}
