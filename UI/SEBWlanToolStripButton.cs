
using SebWindowsClient.DiagnosticsUtils;
using SebWindowsClient.Properties;
using SebWindowsClient.WlanUtils;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace SebWindowsClient.UI
{
  public class SEBWlanToolStripButton : SEBToolStripButton
  {
    private string _oldImage = "";
    private Timer timer;
    private WlanClient wlanClient;
    private WlanClient.WlanInterface wlanInterface;

    public SEBWlanToolStripButton()
    {
      this.Alignment = ToolStripItemAlignment.Right;
      try
      {
        this.wlanClient = new WlanClient();
        this.wlanInterface = this.wlanClient.Interfaces[0];
        this.timer = new Timer();
        this.timer.Tick += (EventHandler) ((x, y) => this.Update());
        this.timer.Interval = 1000;
        this.timer.Start();
      }
      catch (Exception ex)
      {
        Logger.AddError("No WiFi interface found", (object) this, ex, (string) null);
        this.Enabled = false;
        this.Update();
      }
    }

    private void Update()
    {
      try
      {
        if (this.wlanInterface == null)
        {
          this.ChangeImage("nointerface");
          this.ToolTipText = SEBUIStrings.toolTipNoWiFiInterface;
        }
        else if (this.wlanInterface.InterfaceState == Wlan.WlanInterfaceState.Connected)
        {
          int num = this.wlanInterface.InterfaceState == Wlan.WlanInterfaceState.Connected ? this.wlanInterface.RSSI : 0;
          this.UpdateSignalStrength((int) (num > -35 ? new Decimal(100) : (num < -95 ? Decimal.Zero : Math.Round(new Decimal(447392427, 1737688055, 903501810, false, (byte) 28) * ((Decimal) num + new Decimal(95)), 2))));
          this.ToolTipText = string.Format(SEBUIStrings.toolTipConnectedToWiFiNetwork, (object) this.wlanInterface.CurrentConnection.profileName);
        }
        else
        {
          this.ChangeImage("0");
          this.ToolTipText = SEBUIStrings.toolTipNotConnectedToWiFiNetwork;
        }
      }
      catch (Exception ex)
      {
        this.ChangeImage("0");
        this.ToolTipText = SEBUIStrings.toolTipNotConnectedToWiFiNetwork;
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
      int num = (int) new SEBWlanNetworkSelector().ShowDialog();
    }

    private void UpdateSignalStrength(int percentage)
    {
      if (percentage < 16)
        this.ChangeImage("1");
      else if (percentage < 49)
        this.ChangeImage("33");
      else if (percentage < 82)
        this.ChangeImage("66");
      else
        this.ChangeImage("100");
    }

    private void ChangeImage(string imageName)
    {
      if (this._oldImage == imageName)
        return;
      try
      {
        this.Image = (Image) Resources.ResourceManager.GetObject(string.Format("wlan{0}", (object) imageName));
        this._oldImage = imageName;
      }
      catch (Exception ex)
      {
        Logger.AddError("Could not change Image for SEBWLanToolStripButton", (object) this, ex, (string) null);
      }
    }

    private void InitializeComponent()
    {
      this.Margin = new Padding(0, 0, 0, 0);
    }
  }
}
