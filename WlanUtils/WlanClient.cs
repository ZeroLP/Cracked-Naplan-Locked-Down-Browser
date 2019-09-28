
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace SebWindowsClient.WlanUtils
{
  public class WlanClient : IDisposable
  {
    private readonly Dictionary<Guid, WlanClient.WlanInterface> ifaces = new Dictionary<Guid, WlanClient.WlanInterface>();
    private IntPtr clientHandle;
    private uint negotiatedVersion;
    private readonly Wlan.WlanNotificationCallbackDelegate wlanNotificationCallback;

    public WlanClient()
    {
      Wlan.ThrowIfError(Wlan.WlanOpenHandle(1U, IntPtr.Zero, out this.negotiatedVersion, out this.clientHandle));
      try
      {
        this.wlanNotificationCallback = new Wlan.WlanNotificationCallbackDelegate(this.OnWlanNotification);
        Wlan.WlanNotificationSource prevNotifSource;
        Wlan.ThrowIfError(Wlan.WlanRegisterNotification(this.clientHandle, Wlan.WlanNotificationSource.All, false, this.wlanNotificationCallback, IntPtr.Zero, IntPtr.Zero, out prevNotifSource));
      }
      catch
      {
        this.Close();
        throw;
      }
    }

    void IDisposable.Dispose()
    {
      GC.SuppressFinalize((object) this);
      this.Close();
    }

    ~WlanClient()
    {
      this.Close();
    }

    private void Close()
    {
      if (!(this.clientHandle != IntPtr.Zero))
        return;
      Wlan.WlanCloseHandle(this.clientHandle, IntPtr.Zero);
      this.clientHandle = IntPtr.Zero;
    }

    private static Wlan.WlanConnectionNotificationData? ParseWlanConnectionNotification(ref Wlan.WlanNotificationData notifyData)
    {
      int num = Marshal.SizeOf(typeof (Wlan.WlanConnectionNotificationData));
      if (notifyData.dataSize < num)
        return new Wlan.WlanConnectionNotificationData?();
      Wlan.WlanConnectionNotificationData structure = (Wlan.WlanConnectionNotificationData) Marshal.PtrToStructure(notifyData.dataPtr, typeof (Wlan.WlanConnectionNotificationData));
      if (structure.wlanReasonCode == Wlan.WlanReasonCode.Success)
      {
        IntPtr ptr = new IntPtr(notifyData.dataPtr.ToInt64() + Marshal.OffsetOf(typeof (Wlan.WlanConnectionNotificationData), "profileXml").ToInt64());
        structure.profileXml = Marshal.PtrToStringUni(ptr);
      }
      return new Wlan.WlanConnectionNotificationData?(structure);
    }

    private void OnWlanNotification(ref Wlan.WlanNotificationData notifyData, IntPtr context)
    {
      WlanClient.WlanInterface wlanInterface;
      this.ifaces.TryGetValue(notifyData.interfaceGuid, out wlanInterface);
      switch (notifyData.notificationSource)
      {
        case Wlan.WlanNotificationSource.ACM:
          switch ((Wlan.WlanNotificationCodeAcm) notifyData.notificationCode)
          {
            case Wlan.WlanNotificationCodeAcm.ScanFail:
              int num = Marshal.SizeOf(typeof (int));
              if (notifyData.dataSize >= num)
              {
                Wlan.WlanReasonCode reasonCode = (Wlan.WlanReasonCode) Marshal.ReadInt32(notifyData.dataPtr);
                if (wlanInterface != null)
                {
                  wlanInterface.OnWlanReason(notifyData, reasonCode);
                  break;
                }
                break;
              }
              break;
            case Wlan.WlanNotificationCodeAcm.ConnectionStart:
            case Wlan.WlanNotificationCodeAcm.ConnectionComplete:
            case Wlan.WlanNotificationCodeAcm.ConnectionAttemptFail:
            case Wlan.WlanNotificationCodeAcm.Disconnecting:
            case Wlan.WlanNotificationCodeAcm.Disconnected:
              Wlan.WlanConnectionNotificationData? connectionNotification1 = WlanClient.ParseWlanConnectionNotification(ref notifyData);
              if (connectionNotification1.HasValue && wlanInterface != null)
              {
                wlanInterface.OnWlanConnection(notifyData, connectionNotification1.Value);
                break;
              }
              break;
          }
                    break;
        case Wlan.WlanNotificationSource.MSM:
          switch (notifyData.notificationCode)
          {
            case 1:
            case 2:
            case 3:
            case 4:
            case 5:
            case 6:
            case 9:
            case 10:
            case 11:
            case 12:
            case 13:
              Wlan.WlanConnectionNotificationData? connectionNotification2 = WlanClient.ParseWlanConnectionNotification(ref notifyData);
              if (connectionNotification2.HasValue && wlanInterface != null)
              {
                wlanInterface.OnWlanConnection(notifyData, connectionNotification2.Value);
                break;
              }
           break;
          }
                    break;
      }
      if (wlanInterface == null)
        return;
      wlanInterface.OnWlanNotification(notifyData);
    }

    public WlanClient.WlanInterface[] Interfaces
    {
      get
      {
        IntPtr ppInterfaceList;
        Wlan.ThrowIfError(Wlan.WlanEnumInterfaces(this.clientHandle, IntPtr.Zero, out ppInterfaceList));
        try
        {
          Wlan.WlanInterfaceInfoListHeader structure1 = (Wlan.WlanInterfaceInfoListHeader) Marshal.PtrToStructure(ppInterfaceList, typeof (Wlan.WlanInterfaceInfoListHeader));
          long num = ppInterfaceList.ToInt64() + (long) Marshal.SizeOf((object) structure1);
          WlanClient.WlanInterface[] wlanInterfaceArray = new WlanClient.WlanInterface[(int) structure1.numberOfItems];
          List<Guid> guidList = new List<Guid>();
          for (int index = 0; (long) index < (long) structure1.numberOfItems; ++index)
          {
            Wlan.WlanInterfaceInfo structure2 = (Wlan.WlanInterfaceInfo) Marshal.PtrToStructure(new IntPtr(num), typeof (Wlan.WlanInterfaceInfo));
            num += (long) Marshal.SizeOf((object) structure2);
            guidList.Add(structure2.interfaceGuid);
            WlanClient.WlanInterface wlanInterface;
            if (!this.ifaces.TryGetValue(structure2.interfaceGuid, out wlanInterface))
            {
              wlanInterface = new WlanClient.WlanInterface(this, structure2);
              this.ifaces[structure2.interfaceGuid] = wlanInterface;
            }
            wlanInterfaceArray[index] = wlanInterface;
          }
          Queue<Guid> guidQueue = new Queue<Guid>();
          foreach (Guid key in this.ifaces.Keys)
          {
            if (!guidList.Contains(key))
              guidQueue.Enqueue(key);
          }
          while (guidQueue.Count != 0)
            this.ifaces.Remove(guidQueue.Dequeue());
          return wlanInterfaceArray;
        }
        finally
        {
          Wlan.WlanFreeMemory(ppInterfaceList);
        }
      }
    }

    public string GetStringForReasonCode(Wlan.WlanReasonCode reasonCode)
    {
      StringBuilder stringBuffer = new StringBuilder(1024);
      Wlan.ThrowIfError(Wlan.WlanReasonCodeToString(reasonCode, stringBuffer.Capacity, stringBuffer, IntPtr.Zero));
      return stringBuffer.ToString();
    }

    public class WlanInterface
    {
      private readonly AutoResetEvent eventQueueFilled = new AutoResetEvent(false);
      private readonly Queue<object> eventQueue = new Queue<object>();
      private readonly WlanClient client;
      private Wlan.WlanInterfaceInfo info;
      private bool queueEvents;

      public event WlanClient.WlanInterface.WlanNotificationEventHandler WlanNotification;

      public event WlanClient.WlanInterface.WlanConnectionNotificationEventHandler WlanConnectionNotification;

      public event WlanClient.WlanInterface.WlanReasonNotificationEventHandler WlanReasonNotification;

      internal WlanInterface(WlanClient client, Wlan.WlanInterfaceInfo info)
      {
        this.client = client;
        this.info = info;
      }

      private void SetInterfaceInt(Wlan.WlanIntfOpcode opCode, int value)
      {
        IntPtr num = Marshal.AllocHGlobal(4);
        Marshal.WriteInt32(num, value);
        try
        {
          Wlan.ThrowIfError(Wlan.WlanSetInterface(this.client.clientHandle, this.info.interfaceGuid, opCode, 4U, num, IntPtr.Zero));
        }
        finally
        {
          Marshal.FreeHGlobal(num);
        }
      }

      private int GetInterfaceInt(Wlan.WlanIntfOpcode opCode)
      {
        int dataSize;
        IntPtr ppData;
        Wlan.WlanOpcodeValueType wlanOpcodeValueType;
        Wlan.ThrowIfError(Wlan.WlanQueryInterface(this.client.clientHandle, this.info.interfaceGuid, opCode, IntPtr.Zero, out dataSize, out ppData, out wlanOpcodeValueType));
        try
        {
          return Marshal.ReadInt32(ppData);
        }
        finally
        {
          Wlan.WlanFreeMemory(ppData);
        }
      }

      public bool Autoconf
      {
        get
        {
          return (uint) this.GetInterfaceInt(Wlan.WlanIntfOpcode.AutoconfEnabled) > 0U;
        }
        set
        {
          this.SetInterfaceInt(Wlan.WlanIntfOpcode.AutoconfEnabled, value ? 1 : 0);
        }
      }

      public Wlan.Dot11BssType BssType
      {
        get
        {
          return (Wlan.Dot11BssType) this.GetInterfaceInt(Wlan.WlanIntfOpcode.BssType);
        }
        set
        {
          this.SetInterfaceInt(Wlan.WlanIntfOpcode.BssType, (int) value);
        }
      }

      public Wlan.WlanInterfaceState InterfaceState
      {
        get
        {
          return (Wlan.WlanInterfaceState) this.GetInterfaceInt(Wlan.WlanIntfOpcode.InterfaceState);
        }
      }

      public int Channel
      {
        get
        {
          return this.GetInterfaceInt(Wlan.WlanIntfOpcode.ChannelNumber);
        }
      }

      public int RSSI
      {
        get
        {
          return this.GetInterfaceInt(Wlan.WlanIntfOpcode.RSSI);
        }
      }

      public Wlan.WlanRadioState RadioState
      {
        get
        {
          int dataSize;
          IntPtr ppData;
          Wlan.WlanOpcodeValueType wlanOpcodeValueType;
          Wlan.ThrowIfError(Wlan.WlanQueryInterface(this.client.clientHandle, this.info.interfaceGuid, Wlan.WlanIntfOpcode.RadioState, IntPtr.Zero, out dataSize, out ppData, out wlanOpcodeValueType));
          try
          {
            return (Wlan.WlanRadioState) Marshal.PtrToStructure(ppData, typeof (Wlan.WlanRadioState));
          }
          finally
          {
            Wlan.WlanFreeMemory(ppData);
          }
        }
      }

      public Wlan.Dot11OperationMode CurrentOperationMode
      {
        get
        {
          return (Wlan.Dot11OperationMode) this.GetInterfaceInt(Wlan.WlanIntfOpcode.CurrentOperationMode);
        }
      }

      public Wlan.WlanConnectionAttributes CurrentConnection
      {
        get
        {
          int dataSize;
          IntPtr ppData;
          Wlan.WlanOpcodeValueType wlanOpcodeValueType;
          Wlan.ThrowIfError(Wlan.WlanQueryInterface(this.client.clientHandle, this.info.interfaceGuid, Wlan.WlanIntfOpcode.CurrentConnection, IntPtr.Zero, out dataSize, out ppData, out wlanOpcodeValueType));
          try
          {
            return (Wlan.WlanConnectionAttributes) Marshal.PtrToStructure(ppData, typeof (Wlan.WlanConnectionAttributes));
          }
          finally
          {
            Wlan.WlanFreeMemory(ppData);
          }
        }
      }

      public void Scan()
      {
        Wlan.ThrowIfError(Wlan.WlanScan(this.client.clientHandle, this.info.interfaceGuid, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero));
      }

      private static Wlan.WlanAvailableNetwork[] ConvertAvailableNetworkListPtr(IntPtr availNetListPtr)
      {
        Wlan.WlanAvailableNetworkListHeader structure = (Wlan.WlanAvailableNetworkListHeader) Marshal.PtrToStructure(availNetListPtr, typeof (Wlan.WlanAvailableNetworkListHeader));
        long num = availNetListPtr.ToInt64() + (long) Marshal.SizeOf(typeof (Wlan.WlanAvailableNetworkListHeader));
        Wlan.WlanAvailableNetwork[] availableNetworkArray = new Wlan.WlanAvailableNetwork[(int) structure.numberOfItems];
        for (int index = 0; (long) index < (long) structure.numberOfItems; ++index)
        {
          availableNetworkArray[index] = (Wlan.WlanAvailableNetwork) Marshal.PtrToStructure(new IntPtr(num), typeof (Wlan.WlanAvailableNetwork));
          num += (long) Marshal.SizeOf(typeof (Wlan.WlanAvailableNetwork));
        }
        return availableNetworkArray;
      }

      public Wlan.WlanAvailableNetwork[] GetAvailableNetworkList(Wlan.WlanGetAvailableNetworkFlags flags = (Wlan.WlanGetAvailableNetworkFlags) 0)
      {
        IntPtr availableNetworkListPtr;
        Wlan.ThrowIfError(Wlan.WlanGetAvailableNetworkList(this.client.clientHandle, this.info.interfaceGuid, flags, IntPtr.Zero, out availableNetworkListPtr));
        try
        {
          return WlanClient.WlanInterface.ConvertAvailableNetworkListPtr(availableNetworkListPtr);
        }
        finally
        {
          Wlan.WlanFreeMemory(availableNetworkListPtr);
        }
      }

      private static Wlan.WlanBssEntry[] ConvertBssListPtr(IntPtr bssListPtr)
      {
        Wlan.WlanBssListHeader structure = (Wlan.WlanBssListHeader) Marshal.PtrToStructure(bssListPtr, typeof (Wlan.WlanBssListHeader));
        long num = bssListPtr.ToInt64() + (long) Marshal.SizeOf(typeof (Wlan.WlanBssListHeader));
        Wlan.WlanBssEntry[] wlanBssEntryArray = new Wlan.WlanBssEntry[(int) structure.numberOfItems];
        for (int index = 0; (long) index < (long) structure.numberOfItems; ++index)
        {
          wlanBssEntryArray[index] = (Wlan.WlanBssEntry) Marshal.PtrToStructure(new IntPtr(num), typeof (Wlan.WlanBssEntry));
          num += (long) Marshal.SizeOf(typeof (Wlan.WlanBssEntry));
        }
        return wlanBssEntryArray;
      }

      public Wlan.WlanBssEntry[] GetNetworkBssList()
      {
        IntPtr wlanBssList;
        Wlan.ThrowIfError(Wlan.WlanGetNetworkBssList(this.client.clientHandle, this.info.interfaceGuid, IntPtr.Zero, Wlan.Dot11BssType.Any, false, IntPtr.Zero, out wlanBssList));
        try
        {
          return WlanClient.WlanInterface.ConvertBssListPtr(wlanBssList);
        }
        finally
        {
          Wlan.WlanFreeMemory(wlanBssList);
        }
      }

      public Wlan.WlanBssEntry[] GetNetworkBssList(Wlan.Dot11Ssid ssid, Wlan.Dot11BssType bssType, bool securityEnabled)
      {
        IntPtr num = Marshal.AllocHGlobal(Marshal.SizeOf((object) ssid));
        Marshal.StructureToPtr((object) ssid, num, false);
        try
        {
          IntPtr wlanBssList;
          Wlan.ThrowIfError(Wlan.WlanGetNetworkBssList(this.client.clientHandle, this.info.interfaceGuid, num, bssType, securityEnabled, IntPtr.Zero, out wlanBssList));
          try
          {
            return WlanClient.WlanInterface.ConvertBssListPtr(wlanBssList);
          }
          finally
          {
            Wlan.WlanFreeMemory(wlanBssList);
          }
        }
        finally
        {
          Marshal.FreeHGlobal(num);
        }
      }

      protected void Connect(Wlan.WlanConnectionParameters connectionParams)
      {
        Wlan.ThrowIfError(Wlan.WlanConnect(this.client.clientHandle, this.info.interfaceGuid, ref connectionParams, IntPtr.Zero));
      }

      public void Connect(Wlan.WlanConnectionMode connectionMode, Wlan.Dot11BssType bssType, string profile)
      {
        this.Connect(new Wlan.WlanConnectionParameters()
        {
          wlanConnectionMode = connectionMode,
          profile = profile,
          dot11BssType = bssType,
          flags = (Wlan.WlanConnectionFlags) 0
        });
      }

      public bool ConnectSynchronously(Wlan.WlanConnectionMode connectionMode, Wlan.Dot11BssType bssType, string profile, int connectTimeout)
      {
        this.queueEvents = true;
        try
        {
          this.Connect(connectionMode, bssType, profile);
          while (this.queueEvents)
          {
            if (this.eventQueueFilled.WaitOne(connectTimeout, true))
            {
              lock (this.eventQueue)
              {
                while (this.eventQueue.Count != 0)
                {
                  object obj = this.eventQueue.Dequeue();
                  if (obj is WlanClient.WlanInterface.WlanConnectionNotificationEventData)
                  {
                    WlanClient.WlanInterface.WlanConnectionNotificationEventData notificationEventData = (WlanClient.WlanInterface.WlanConnectionNotificationEventData) obj;
                    if (notificationEventData.notifyData.notificationSource == Wlan.WlanNotificationSource.ACM)
                    {
                      if (notificationEventData.notifyData.notificationCode == 10)
                      {
                        if (notificationEventData.connNotifyData.profileName == profile)
                          return true;
                        break;
                      }
                      break;
                    }
                    break;
                  }
                }
              }
            }
            else
              break;
          }
        }
        finally
        {
          this.queueEvents = false;
          this.eventQueue.Clear();
        }
        return false;
      }

      public void Connect(Wlan.WlanConnectionMode connectionMode, Wlan.Dot11BssType bssType, Wlan.Dot11Ssid ssid, Wlan.WlanConnectionFlags flags)
      {
        Wlan.WlanConnectionParameters connectionParams = new Wlan.WlanConnectionParameters();
        connectionParams.wlanConnectionMode = connectionMode;
        connectionParams.dot11SsidPtr = Marshal.AllocHGlobal(Marshal.SizeOf((object) ssid));
        Marshal.StructureToPtr((object) ssid, connectionParams.dot11SsidPtr, false);
        connectionParams.dot11BssType = bssType;
        connectionParams.flags = flags;
        this.Connect(connectionParams);
        Marshal.DestroyStructure(connectionParams.dot11SsidPtr, ssid.GetType());
        Marshal.FreeHGlobal(connectionParams.dot11SsidPtr);
      }

      public void DeleteProfile(string profileName)
      {
        Wlan.ThrowIfError(Wlan.WlanDeleteProfile(this.client.clientHandle, this.info.interfaceGuid, profileName, IntPtr.Zero));
      }

      public Wlan.WlanReasonCode SetProfile(Wlan.WlanProfileFlags flags, string profileXml, bool overwrite)
      {
        Wlan.WlanReasonCode reasonCode;
        Wlan.ThrowIfError(Wlan.WlanSetProfile(this.client.clientHandle, this.info.interfaceGuid, flags, profileXml, (string) null, overwrite, IntPtr.Zero, out reasonCode));
        return reasonCode;
      }

      public string GetProfileXml(string profileName)
      {
        IntPtr profileXml;
        Wlan.WlanProfileFlags flags;
        Wlan.WlanAccess grantedAccess;
        Wlan.ThrowIfError(Wlan.WlanGetProfile(this.client.clientHandle, this.info.interfaceGuid, profileName, IntPtr.Zero, out profileXml, out flags, out grantedAccess));
        try
        {
          return Marshal.PtrToStringUni(profileXml);
        }
        finally
        {
          Wlan.WlanFreeMemory(profileXml);
        }
      }

      public Wlan.WlanProfileInfo[] GetProfiles()
      {
        IntPtr profileList;
        Wlan.ThrowIfError(Wlan.WlanGetProfileList(this.client.clientHandle, this.info.interfaceGuid, IntPtr.Zero, out profileList));
        try
        {
          Wlan.WlanProfileInfoListHeader structure1 = (Wlan.WlanProfileInfoListHeader) Marshal.PtrToStructure(profileList, typeof (Wlan.WlanProfileInfoListHeader));
          Wlan.WlanProfileInfo[] wlanProfileInfoArray = new Wlan.WlanProfileInfo[(int) structure1.numberOfItems];
          long num = profileList.ToInt64() + (long) Marshal.SizeOf((object) structure1);
          for (int index = 0; (long) index < (long) structure1.numberOfItems; ++index)
          {
            Wlan.WlanProfileInfo structure2 = (Wlan.WlanProfileInfo) Marshal.PtrToStructure(new IntPtr(num), typeof (Wlan.WlanProfileInfo));
            wlanProfileInfoArray[index] = structure2;
            num += (long) Marshal.SizeOf((object) structure2);
          }
          return wlanProfileInfoArray;
        }
        finally
        {
          Wlan.WlanFreeMemory(profileList);
        }
      }

      internal void OnWlanConnection(Wlan.WlanNotificationData notifyData, Wlan.WlanConnectionNotificationData connNotifyData)
      {
        // ISSUE: reference to a compiler-generated field
        if (this.WlanConnectionNotification != null)
        {
          // ISSUE: reference to a compiler-generated field
          this.WlanConnectionNotification(notifyData, connNotifyData);
        }
        if (!this.queueEvents)
          return;
        this.EnqueueEvent((object) new WlanClient.WlanInterface.WlanConnectionNotificationEventData()
        {
          notifyData = notifyData,
          connNotifyData = connNotifyData
        });
      }

      internal void OnWlanReason(Wlan.WlanNotificationData notifyData, Wlan.WlanReasonCode reasonCode)
      {
        // ISSUE: reference to a compiler-generated field
        if (this.WlanReasonNotification != null)
        {
          // ISSUE: reference to a compiler-generated field
          this.WlanReasonNotification(notifyData, reasonCode);
        }
        if (!this.queueEvents)
          return;
        this.EnqueueEvent((object) new WlanClient.WlanInterface.WlanReasonNotificationData()
        {
          notifyData = notifyData,
          reasonCode = reasonCode
        });
      }

      internal void OnWlanNotification(Wlan.WlanNotificationData notifyData)
      {
        // ISSUE: reference to a compiler-generated field
        if (this.WlanNotification == null)
          return;
        // ISSUE: reference to a compiler-generated field
        this.WlanNotification(notifyData);
      }

      private void EnqueueEvent(object queuedEvent)
      {
        lock (this.eventQueue)
          this.eventQueue.Enqueue(queuedEvent);
        this.eventQueueFilled.Set();
      }

      public NetworkInterface NetworkInterface
      {
        get
        {
          foreach (NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces())
          {
            if (new Guid(networkInterface.Id).Equals(this.info.interfaceGuid))
              return networkInterface;
          }
          return (NetworkInterface) null;
        }
      }

      public Guid InterfaceGuid
      {
        get
        {
          return this.info.interfaceGuid;
        }
      }

      public string InterfaceDescription
      {
        get
        {
          return this.info.interfaceDescription;
        }
      }

      public string InterfaceName
      {
        get
        {
          return this.NetworkInterface.Name;
        }
      }

      public delegate void WlanNotificationEventHandler(Wlan.WlanNotificationData notifyData);

      public delegate void WlanConnectionNotificationEventHandler(Wlan.WlanNotificationData notifyData, Wlan.WlanConnectionNotificationData connNotifyData);

      public delegate void WlanReasonNotificationEventHandler(Wlan.WlanNotificationData notifyData, Wlan.WlanReasonCode reasonCode);

      private struct WlanConnectionNotificationEventData
      {
        public Wlan.WlanNotificationData notifyData;
        public Wlan.WlanConnectionNotificationData connNotifyData;
      }

      private struct WlanReasonNotificationData
      {
        public Wlan.WlanNotificationData notifyData;
        public Wlan.WlanReasonCode reasonCode;
      }
    }
  }
}
