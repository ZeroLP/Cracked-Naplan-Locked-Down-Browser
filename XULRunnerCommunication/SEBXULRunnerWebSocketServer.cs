
using Fleck;
using SebWindowsClient.ConfigurationUtils;
using SebWindowsClient.DiagnosticsUtils;
using System;
using System.Net.NetworkInformation;
using System.Threading;
using System.Windows.Forms;

namespace SebWindowsClient.XULRunnerCommunication
{
  public class SEBXULRunnerWebSocketServer
  {
    public static bool Started = false;
    private static int port = 8706;
    private static IWebSocketConnection XULRunner;
    private static WebSocketServer server;

    public static string ServerAddress
    {
      get
      {
        return string.Format("ws://localhost:{0}", (object) SEBXULRunnerWebSocketServer.port);
      }
    }

    public static bool IsRunning
    {
      get
      {
        if (SEBXULRunnerWebSocketServer.server != null)
          return true;
        foreach (TcpConnectionInformation activeTcpConnection in IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections())
        {
          if (activeTcpConnection.LocalEndPoint.Port == SEBXULRunnerWebSocketServer.port && activeTcpConnection.State != TcpState.TimeWait)
          {
            Logger.AddInformation("Server already running!", (object) null, (Exception) null, (string) null);
            return true;
          }
        }
        return false;
      }
    }

    public static event EventHandler OnXulRunnerCloseRequested;

    public static event EventHandler OnXulRunnerQuitLinkClicked;

    public static event EventHandler OnXulRunnerTextFocus;

    public static event EventHandler OnXulRunnerTextBlur;

    public static void StartServer()
    {
      if (SEBXULRunnerWebSocketServer.IsRunning && SEBXULRunnerWebSocketServer.Started)
        return;
      if (SEBXULRunnerWebSocketServer.IsRunning)
      {
        for (int index = 0; index < 60 && SEBXULRunnerWebSocketServer.IsRunning; ++index)
          Thread.Sleep(1000);
        if (SEBXULRunnerWebSocketServer.IsRunning)
        {
          int num = (int) SEBMessageBox.Show(SEBUIStrings.alertWebSocketPortBlocked, SEBUIStrings.alertWebSocketPortBlockedMessage, MessageBoxIcon.Hand, MessageBoxButtons.OK, false);
        }
      }
      try
      {
        Logger.AddInformation("Starting WebSocketServer on " + SEBXULRunnerWebSocketServer.ServerAddress, (object) null, (Exception) null, (string) null);
        SEBXULRunnerWebSocketServer.server = new WebSocketServer(SEBXULRunnerWebSocketServer.ServerAddress);
        FleckLog.Level = Fleck.LogLevel.Debug;
        SEBXULRunnerWebSocketServer.server.Start((Action<IWebSocketConnection>) (socket =>
        {
          socket.OnOpen = (Action) (() => SEBXULRunnerWebSocketServer.OnClientConnected(socket));
          socket.OnClose = new Action(SEBXULRunnerWebSocketServer.OnClientDisconnected);
          socket.OnMessage = new Action<string>(SEBXULRunnerWebSocketServer.OnClientMessage);
        }));
        Logger.AddInformation("Started WebSocketServer on " + SEBXULRunnerWebSocketServer.ServerAddress, (object) null, (Exception) null, (string) null);
        SEBXULRunnerWebSocketServer.Started = true;
      }
      catch (Exception ex)
      {
        Logger.AddError("Unable to start WebSocketsServer for communication with XULRunner", (object) null, ex, (string) null);
      }
    }

    private static void OnClientDisconnected()
    {
      Logger.AddInformation("WebSocket: Client disconnected", (object) null, (Exception) null, (string) null);
      SEBXULRunnerWebSocketServer.XULRunner = (IWebSocketConnection) null;
    }

    private static void OnClientConnected(IWebSocketConnection socket)
    {
      Logger.AddInformation("WebSocket: Client Connectedon port:" + (object) socket.ConnectionInfo.ClientPort, (object) null, (Exception) null, (string) null);
      SEBXULRunnerWebSocketServer.XULRunner = socket;
    }

    public static void SendAllowCloseToXulRunner()
    {
      try
      {
        if (SEBXULRunnerWebSocketServer.XULRunner == null)
          return;
        Console.WriteLine("SEB.Close sent");
        Logger.AddInformation("WebSocket: Send message: SEB.close", (object) null, (Exception) null, (string) null);
        SEBXULRunnerWebSocketServer.XULRunner.Send("SEB.close");
      }
      catch (Exception ex)
      {
      }
    }

    public static void SendRestartExam()
    {
      try
      {
        if (SEBXULRunnerWebSocketServer.XULRunner == null || string.IsNullOrEmpty((string) SEBClientInfo.getSebSetting("restartExamURL")["restartExamURL"]) && !(bool) SEBClientInfo.getSebSetting("restartExamUseStartURL")["restartExamUseStartURL"])
          return;
        Console.WriteLine("SEB.Restart Exam sent");
        Logger.AddInformation("WebSocket: Send message: SEB.restartExam", (object) null, (Exception) null, (string) null);
        SEBXULRunnerWebSocketServer.XULRunner.Send("SEB.restartExam");
      }
      catch (Exception ex)
      {
      }
    }

    public static void SendReloadPage()
    {
      try
      {
        if (SEBXULRunnerWebSocketServer.XULRunner == null)
          return;
        Console.WriteLine("SEB.Reload Sent");
        Logger.AddInformation("WebSocket: Send message: SEB.reload", (object) null, (Exception) null, (string) null);
        SEBXULRunnerWebSocketServer.XULRunner.Send("SEB.reload");
      }
      catch (Exception ex)
      {
      }
    }

    private static void OnClientMessage(string message)
    {
      Console.WriteLine("RECV: " + message);
      Logger.AddInformation("WebSocket: Received message: " + message, (object) null, (Exception) null, (string) null);
      if (!(message == "seb.beforeclose.manual"))
      {
        if (!(message == "seb.beforeclose.quiturl"))
        {
          if (!(message == "seb.input.focus"))
          {
            // ISSUE: reference to a compiler-generated field
            if (!(message == "seb.input.blur") || SEBXULRunnerWebSocketServer.OnXulRunnerTextBlur == null)
              return;
            // ISSUE: reference to a compiler-generated field
            SEBXULRunnerWebSocketServer.OnXulRunnerTextBlur((object) null, EventArgs.Empty);
          }
          else
          {
            // ISSUE: reference to a compiler-generated field
            if (SEBXULRunnerWebSocketServer.OnXulRunnerTextFocus == null)
              return;
            // ISSUE: reference to a compiler-generated field
            SEBXULRunnerWebSocketServer.OnXulRunnerTextFocus((object) null, EventArgs.Empty);
          }
        }
        else
        {
          // ISSUE: reference to a compiler-generated field
          if (SEBXULRunnerWebSocketServer.OnXulRunnerQuitLinkClicked == null)
            return;
          // ISSUE: reference to a compiler-generated field
          SEBXULRunnerWebSocketServer.OnXulRunnerQuitLinkClicked((object) null, EventArgs.Empty);
        }
      }
      else
      {
        // ISSUE: reference to a compiler-generated field
        if (SEBXULRunnerWebSocketServer.OnXulRunnerCloseRequested == null)
          return;
        // ISSUE: reference to a compiler-generated field
        SEBXULRunnerWebSocketServer.OnXulRunnerCloseRequested((object) null, EventArgs.Empty);
      }
    }

    public static void SendDisplaySettingsChanged()
    {
      try
      {
        if (SEBXULRunnerWebSocketServer.XULRunner == null)
          return;
        Console.WriteLine("SEB.ChangedDisplaySettingsChanged");
        Logger.AddInformation("WebSocket: Send message: SEB.displaySettingsChanged", (object) null, (Exception) null, (string) null);
        SEBXULRunnerWebSocketServer.XULRunner.Send("SEB.displaySettingsChanged");
      }
      catch (Exception ex)
      {
      }
    }

    public static void SendKeyboardShown()
    {
      try
      {
        if (SEBXULRunnerWebSocketServer.XULRunner == null)
          return;
        Console.WriteLine("SEB.keyboardShown");
        Logger.AddInformation("WebSocket: Send message: SEB.keyboardShown", (object) null, (Exception) null, (string) null);
        SEBXULRunnerWebSocketServer.XULRunner.Send("SEB.keyboardShown");
      }
      catch (Exception ex)
      {
      }
    }
  }
}
