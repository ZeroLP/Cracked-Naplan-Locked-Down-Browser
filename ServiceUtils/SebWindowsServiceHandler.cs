

using SebWindowsClient.ConfigurationUtils;
using SebWindowsClient.DiagnosticsUtils;
using SEBWindowsServiceContracts;
using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace SebWindowsClient.ServiceUtils
{
  public static class SebWindowsServiceHandler
  {
    private static bool _initialized;
    private static string _username;
    private static string _sid;
    private static IRegistryServiceContract _sebWindowsServicePipeProxy;
    private static InputParamsManager.Parameters _oldInputParams;

    private static void Initialize()
    {
      if (SebWindowsServiceHandler._initialized)
        return;
      Logger.AddInformation("initializing wcf service connection", (object) null, (Exception) null, (string) null);
      SebWindowsServiceHandler._sebWindowsServicePipeProxy = new ChannelFactory<IRegistryServiceContract>((Binding) new NetNamedPipeBinding(NetNamedPipeSecurityMode.Transport), new EndpointAddress("net.pipe://localhost/SebWindowsServiceWCF/service")).CreateChannel();
      if (string.IsNullOrEmpty(SebWindowsServiceHandler._username))
        SebWindowsServiceHandler._username = SebWindowsServiceHandler.GetCurrentUsername();
      if (string.IsNullOrEmpty(SebWindowsServiceHandler._sid))
        SebWindowsServiceHandler._sid = SebWindowsServiceHandler.GetCurrentUserSID();
      if (string.IsNullOrEmpty(SebWindowsServiceHandler._sid) && string.IsNullOrEmpty(SebWindowsServiceHandler._username))
        throw new Exception("Unable to get SID & Username");
      SebWindowsServiceHandler._initialized = true;
    }

    private static string GetCurrentUserSID()
    {
      try
      {
        WindowsIdentity current = WindowsIdentity.GetCurrent();
        if (current != null && current.User != (SecurityIdentifier) null)
          return current.User.Value;
        Logger.AddWarning("Unable to get SID from WindowsIdentity", (object) null, (Exception) null, (string) null);
      }
      catch (Exception ex)
      {
        Logger.AddWarning("Unable to get SID from WindowsIdentity", (object) null, ex, (string) null);
      }
      return (string) null;
    }

    private static string GetCurrentUsername()
    {
      try
      {
        string userName = Environment.UserName;
        if (string.IsNullOrEmpty(userName))
        {
          Logger.AddWarning("Unable to get Username from Environment", (object) null, (Exception) null, (string) null);
        }
        else
        {
          Logger.AddInformation("Username from Environment = " + userName, (object) null, (Exception) null, (string) null);
          return userName;
        }
      }
      catch (Exception ex)
      {
        Logger.AddWarning("Unable to get Username from Environment", (object) null, ex, (string) null);
      }
      return (string) null;
    }

    public static bool SetRegistryAccordingToConfiguration()
    {
      return SebWindowsServiceHandler.SetRegistryAccordingToConfiguration(new Dictionary<RegistryIdentifiers, object>()
      {
        {
          RegistryIdentifiers.DisableLockWorkstation,
          (object) ((bool) SEBClientInfo.getSebSetting("insideSebEnableLockThisComputer")["insideSebEnableLockThisComputer"] ? 0 : 1)
        },
        {
          RegistryIdentifiers.DisableChangePassword,
          (object) ((bool) SEBClientInfo.getSebSetting("insideSebEnableChangeAPassword")["insideSebEnableChangeAPassword"] ? 0 : 1)
        },
        {
          RegistryIdentifiers.DisableTaskMgr,
          (object) ((bool) SEBClientInfo.getSebSetting("insideSebEnableStartTaskManager")["insideSebEnableStartTaskManager"] ? 0 : 1)
        },
        {
          RegistryIdentifiers.HideFastUserSwitching,
          (object) ((bool) SEBClientInfo.getSebSetting("insideSebEnableSwitchUser")["insideSebEnableSwitchUser"] ? 0 : 1)
        },
        {
          RegistryIdentifiers.NoLogoff,
          (object) ((bool) SEBClientInfo.getSebSetting("insideSebEnableLogOff")["insideSebEnableLogOff"] ? 0 : 1)
        },
        {
          RegistryIdentifiers.NoClose,
          (object) ((bool) SEBClientInfo.getSebSetting("insideSebEnableShutDown")["insideSebEnableShutDown"] ? 0 : 1)
        },
        {
          RegistryIdentifiers.EnableShade,
          (object) ((bool) SEBClientInfo.getSebSetting("insideSebEnableVmWareClientShade")["insideSebEnableVmWareClientShade"] ? 1 : 0)
        },
        {
          RegistryIdentifiers.EnableShadeHorizon,
          (object) ((bool) SEBClientInfo.getSebSetting("insideSebEnableVmWareClientShade")["insideSebEnableVmWareClientShade"] ? "True" : "False")
        },
        {
          RegistryIdentifiers.EaseOfAccess,
          (object) ((bool) SEBClientInfo.getSebSetting("insideSebEnableEaseOfAccess")["insideSebEnableEaseOfAccess"] ? "" : "SebDummy.exe")
        },
        {
          RegistryIdentifiers.DontDisplayNetworkSelectionUI,
          (object) ((bool) SEBClientInfo.getSebSetting("insideSebEnableEnableNetworkConnectionSelector")["insideSebEnableEnableNetworkConnectionSelector"] ? 0 : 1)
        }
      });
    }

    public static bool SetRegistryAccordingToConfiguration(Dictionary<RegistryIdentifiers, object> valuesToSet)
    {
      SebWindowsServiceHandler.Initialize();
      SebWindowsServiceHandler.SetInputParameters();
      return SebWindowsServiceHandler._sebWindowsServicePipeProxy.SetRegistryEntries(valuesToSet, SebWindowsServiceHandler._sid, SebWindowsServiceHandler._username);
    }

    public static bool ResetRegistry()
    {
      Logger.AddInformation("resetting registry entries", (object) null, (Exception) null, (string) null);
      SebWindowsServiceHandler.Initialize();
      Logger.AddInformation("calling reset on wcf service", (object) null, (Exception) null, (string) null);
      SebWindowsServiceHandler.ResetInputParameters();
      return SebWindowsServiceHandler._sebWindowsServicePipeProxy.Reset();
    }

    public static bool DisableWindowsUpdate()
    {
      Logger.AddInformation("calling disable windows update on wcf service", (object) null, (Exception) null, (string) null);
      return SebWindowsServiceHandler._sebWindowsServicePipeProxy.DisableWindowsUpdate();
    }

    public static bool IsServiceAvailable
    {
      get
      {
        try
        {
          SebWindowsServiceHandler.Initialize();
          if (SebWindowsServiceHandler._sebWindowsServicePipeProxy.TestServiceConnetcion())
          {
            Logger.AddInformation("SEB Windows service available", (object) null, (Exception) null, (string) null);
            return true;
          }
          Logger.AddInformation("SEB Windows service not available", (object) null, (Exception) null, (string) null);
          return false;
        }
        catch (Exception ex)
        {
          Logger.AddInformation("SEB Windows service not available", (object) ex, (Exception) null, (string) null);
          return false;
        }
      }
    }

    public static void Reconnect()
    {
      SebWindowsServiceHandler._initialized = false;
      SebWindowsServiceHandler.Initialize();
    }

    private static void SetInputParameters()
    {
      SebWindowsServiceHandler._oldInputParams = InputParamsManager.Get();
      InputParamsManager.Parameters parameters = (InputParamsManager.Parameters) SebWindowsServiceHandler._oldInputParams.Clone();
      bool? nullable = new bool?(false);
      parameters.IsPredictionEnabled = nullable;
      InputParamsManager.Set(parameters);
    }

    private static void ResetInputParameters()
    {
      if (SebWindowsServiceHandler._oldInputParams == null)
        return;
      InputParamsManager.Set(SebWindowsServiceHandler._oldInputParams);
    }
  }
}
