

using SebWindowsClient.ConfigurationUtils;
using SebWindowsClient.DiagnosticsUtils;
using SebWindowsClient.Properties;
using SebWindowsClient.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SebWindowsClient.WlanUtils
{
  public class SEBWlanNetworkSelector : Form
  {
    private WlanClient.WlanInterface wlanInterface;
    private WlanClient wlanClient;
    private List<Wlan.WlanAvailableNetwork> availableNetworks;
    private IContainer components;
    private ListBox listNetworks;
    private Button buttonConnect;
    private Button buttonCancel;
    private Button buttonRefresh;

    public SEBWlanNetworkSelector()
    {
      this.InitializeComponent();
      try
      {
        this.wlanClient = new WlanClient();
        this.wlanInterface = this.wlanClient.Interfaces[0];
        this.RefreshNetworks();
      }
      catch (Exception ex)
      {
        this.listNetworks.Enabled = false;
        this.listNetworks.Items.Add((object) SEBUIStrings.WlanNoNetworkInterfaceFound);
        Logger.AddError("No Network interface found", (object) this, ex, (string) null);
      }
    }

    private void RefreshNetworks()
    {
      this.listNetworks.DataSource = (object) null;
      this.listNetworks.Items.Clear();
      try
      {
        this.availableNetworks = ((IEnumerable<Wlan.WlanAvailableNetwork>) this.wlanInterface.GetAvailableNetworkList(Wlan.WlanGetAvailableNetworkFlags.IncludeAllAdhocProfiles)).Where<Wlan.WlanAvailableNetwork>((Func<Wlan.WlanAvailableNetwork, bool>) (network => !string.IsNullOrWhiteSpace(network.profileName))).ToList<Wlan.WlanAvailableNetwork>();
        if (this.availableNetworks.Count<Wlan.WlanAvailableNetwork>() <= 0)
          throw new Exception("No networks found!");
        for (int index = 0; index < this.availableNetworks.Count; ++index)
        {
          Wlan.WlanAvailableNetwork availableNetwork = this.availableNetworks[index];
          this.listNetworks.Items.Add((object) string.Format("{0} ({1})", (object) availableNetwork.dot11Ssid.GetSSID(), (object) availableNetwork.wlanSignalQuality));
          try
          {
            if (this.wlanInterface.CurrentConnection.profileName == availableNetwork.profileName)
              this.listNetworks.SelectedIndex = index;
          }
          catch (Exception ex)
          {
          }
        }
      }
      catch (Exception ex)
      {
        this.listNetworks.Enabled = false;
        this.listNetworks.Items.Add((object) SEBUIStrings.WlanNoNetworksFound);
        this.listNetworks.Items.Add((object) SEBUIStrings.WlanYouCanOnlyConnectToNetworks);
        this.listNetworks.Items.Add((object) SEBUIStrings.WlanThatYouHaveUsedBefore);
        Logger.AddError("No Networks found", (object) this, ex, (string) null);
      }
    }

    private void buttonConnect_Click(object sender, EventArgs e)
    {
      string profileName = this.availableNetworks[this.listNetworks.SelectedIndex].profileName;
      this.buttonConnect.Text = SEBUIStrings.WlanConnecting;
      this.buttonConnect.Enabled = false;
      this.listNetworks.Enabled = false;
      try
      {
        int num = (int) this.wlanInterface.SetProfile(Wlan.WlanProfileFlags.AllUser, this.wlanInterface.GetProfileXml(profileName), true);
        this.wlanInterface.Connect(Wlan.WlanConnectionMode.Profile, Wlan.Dot11BssType.Any, profileName);
        this.Close();
      }
      catch (Exception ex)
      {
        int num = (int) SEBMessageBox.Show(SEBUIStrings.WlanConnectionFailedMessageTitle, SEBUIStrings.WlanConnectionFailedMessage, MessageBoxIcon.Hand, MessageBoxButtons.OK, false);
        Logger.AddError(ex.Message, (object) this, ex, (string) null);
      }
    }

    private void listNetworks_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (this.listNetworks.SelectedItem == null)
        return;
      this.buttonConnect.Enabled = true;
    }

    private void buttonCancel_Click(object sender, EventArgs e)
    {
      this.Close();
    }

    private void buttonRefresh_Click(object sender, EventArgs e)
    {
      this.RefreshNetworks();
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof (SEBWlanNetworkSelector));
      this.listNetworks = new ListBox();
      this.buttonConnect = new Button();
      this.buttonCancel = new Button();
      this.buttonRefresh = new Button();
      this.SuspendLayout();
      ListBox listNetworks = this.listNetworks;
      string objectName1 = "listNetworks";
      componentResourceManager.ApplyResources((object) listNetworks, objectName1);
      this.listNetworks.BackColor = SystemColors.Window;
      this.listNetworks.FormattingEnabled = true;
      this.listNetworks.Name = "listNetworks";
      this.listNetworks.SelectedIndexChanged += new EventHandler(this.listNetworks_SelectedIndexChanged);
      Button buttonConnect = this.buttonConnect;
      string objectName2 = "buttonConnect";
      componentResourceManager.ApplyResources((object) buttonConnect, objectName2);
      this.buttonConnect.Name = "buttonConnect";
      this.buttonConnect.UseVisualStyleBackColor = true;
      this.buttonConnect.Click += new EventHandler(this.buttonConnect_Click);
      Button buttonCancel = this.buttonCancel;
      string objectName3 = "buttonCancel";
      componentResourceManager.ApplyResources((object) buttonCancel, objectName3);
      this.buttonCancel.Name = "buttonCancel";
      this.buttonCancel.UseVisualStyleBackColor = true;
      this.buttonCancel.Click += new EventHandler(this.buttonCancel_Click);
      Button buttonRefresh = this.buttonRefresh;
      string objectName4 = "buttonRefresh";
      componentResourceManager.ApplyResources((object) buttonRefresh, objectName4);
      this.buttonRefresh.BackgroundImage = (Image) Resources.refresh;
      this.buttonRefresh.FlatAppearance.BorderSize = 0;
      this.buttonRefresh.Name = "buttonRefresh";
      this.buttonRefresh.UseVisualStyleBackColor = true;
      this.buttonRefresh.Click += new EventHandler(this.buttonRefresh_Click);
      string objectName5 = "$this";
      componentResourceManager.ApplyResources((object) this, objectName5);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.Controls.Add((Control) this.buttonRefresh);
      this.Controls.Add((Control) this.buttonCancel);
      this.Controls.Add((Control) this.buttonConnect);
      this.Controls.Add((Control) this.listNetworks);
      this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
      this.Name = nameof (SEBWlanNetworkSelector);
      this.TopMost = true;
      this.ResumeLayout(false);
    }
  }
}
