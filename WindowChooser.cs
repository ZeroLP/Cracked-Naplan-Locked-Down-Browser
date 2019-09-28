
using SebWindowsClient.ConfigurationUtils;
using SebWindowsClient.ProcessUtils;
using SebWindowsClient.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SebWindowsClient
{
  public class WindowChooser : Form
  {
    private static uint WM_GETICON = (uint) sbyte.MaxValue;
    private static IntPtr ICON_SMALL2 = new IntPtr(2);
    private static int GCL_HICON = -14;
    private Process _process;
    private List<KeyValuePair<IntPtr, string>> _openedWindows;
    private IContainer components;
    private ListView appList;
    private ListView closeListView;

    public WindowChooser(Process process, int left, int top)
    {
      this.Left = 0;
      this.InitializeComponent();
      this.appList.Click += new EventHandler(this.ShowWindow);
      int num1 = (int) (82.0 * (double) SEBClientInfo.scaleFactor);
      int num2 = (int) (32.0 * (double) SEBClientInfo.scaleFactor);
      int height = (int) (24.0 * (double) SEBClientInfo.scaleFactor);
      try
      {
        this._process = process;
        ImageList imageList1 = new ImageList();
        ImageList imageList2 = (ImageList) null;
        if ((bool) SEBClientInfo.getSebSetting("touchOptimized")["touchOptimized"])
        {
          int num3 = (int) (48.0 * (double) SEBClientInfo.scaleFactor);
          imageList1.ImageSize = new Size(num3, num3);
          imageList2 = new ImageList();
          imageList2.ImageSize = new Size(num3, height);
          imageList2.ColorDepth = ColorDepth.Depth32Bit;
          this.closeListView.Click += new EventHandler(this.CloseWindow);
          this.appList.Height = num1;
          this.closeListView.Height = height + 12;
        }
        else
        {
          imageList1.ImageSize = new Size(num2, num2);
          this.appList.Height = num1;
        }
        imageList1.ColorDepth = ColorDepth.Depth32Bit;
        this._openedWindows = process.GetOpenWindows().ToList<KeyValuePair<IntPtr, string>>();
        if (this._process.MainWindowHandle != IntPtr.Zero && !this._openedWindows.Any<KeyValuePair<IntPtr, string>>((Func<KeyValuePair<IntPtr, string>, bool>) (oW => oW.Key == this._process.MainWindowHandle)))
          this._openedWindows.Add(new KeyValuePair<IntPtr, string>(this._process.MainWindowHandle, this._process.MainWindowTitle));
        if (this._openedWindows.Count == 1)
          this.ShowWindow(this._openedWindows.First<KeyValuePair<IntPtr, string>>().Key);
        int imageIndex = 0;
        foreach (KeyValuePair<IntPtr, string> openedWindow in this._openedWindows)
        {
          Image smallWindowIcon = WindowChooser.GetSmallWindowIcon(openedWindow.Key);
          imageList1.Images.Add(smallWindowIcon);
          this.appList.Items.Add(new ListViewItem(openedWindow.Value, imageIndex));
          if (imageList2 != null)
          {
            imageList2.Images.Add((Image) Resources.closewindow);
            this.closeListView.Items.Add(new ListViewItem("close", imageIndex));
          }
          ++imageIndex;
        }
        if (imageList2 != null)
        {
          this.closeListView.View = View.LargeIcon;
          this.closeListView.LargeImageList = imageList2;
        }
        this.appList.View = View.LargeIcon;
        this.appList.LargeImageList = imageList1;
        this.Height = 6 + this.appList.Size.Height + (imageList2 != null ? height : 0) + 6;
        this.Top = top - this.Height;
      }
      catch (Exception ex)
      {
        this.Close();
      }
      this.Width = Screen.PrimaryScreen.Bounds.Width;
      this.Show();
      this.appList.Focus();
      Timer timer = new Timer();
      timer.Tick += new EventHandler(this.CloseIt);
      timer.Interval = !(bool) SEBSettings.settingsCurrent["touchOptimized"] ? 3000 : 4000;
      timer.Start();
    }

    private void CloseIt(object sender, EventArgs e)
    {
      this.Close();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
      Console.WriteLine("Closing");
      this.appList.Click -= new EventHandler(this.ShowWindow);
      base.OnClosing(e);
    }

    private void ShowWindow(object sender, EventArgs e)
    {
      this.ShowWindow(this._openedWindows[this.appList.SelectedIndices[0]].Key);
    }

    private void CloseWindow(object sender, EventArgs e)
    {
      this._openedWindows[this.closeListView.SelectedIndices[0]].Key.CloseWindow();
      this.Close();
    }

    private void ShowWindow(IntPtr windowHandle)
    {
      windowHandle.BringToTop(true);
      if ((bool) SEBClientInfo.getSebSetting("touchOptimized")["touchOptimized"])
        this._openedWindows.First<KeyValuePair<IntPtr, string>>().Key.MaximizeWindow();
      this.Close();
    }

    public static Image GetSmallWindowIcon(IntPtr hWnd)
    {
      try
      {
        try
        {
          Process process = hWnd.GetProcess();
          if (process.ProcessName.Contains("xulrunner"))
            return (Image) Icon.ExtractAssociatedIcon(Application.ExecutablePath).ToBitmap();
          return (Image) Icon.ExtractAssociatedIcon(process.Modules[0].FileName).ToBitmap();
        }
        catch
        {
        }
        IntPtr num = new IntPtr();
        IntPtr handle = WindowChooser.SendMessage(hWnd, WindowChooser.WM_GETICON, WindowChooser.ICON_SMALL2, IntPtr.Zero);
        if (handle == IntPtr.Zero)
          handle = WindowChooser.GetClassLongPtr(hWnd, WindowChooser.GCL_HICON);
        if (handle == IntPtr.Zero)
          handle = WindowChooser.LoadIcon(IntPtr.Zero, (IntPtr) 32512);
        if (handle != IntPtr.Zero)
          return (Image) new Bitmap((Image) Icon.FromHandle(handle).ToBitmap(), 32, 32);
        return (Image) null;
      }
      catch (Exception ex)
      {
        return (Image) null;
      }
    }

    private static IntPtr GetClassLongPtr(IntPtr hWnd, int nIndex)
    {
      if (IntPtr.Size == 4)
        return new IntPtr((long) WindowChooser.GetClassLong32(hWnd, nIndex));
      return WindowChooser.GetClassLong64(hWnd, nIndex);
    }

    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern IntPtr LoadIcon(IntPtr hInstance, IntPtr lpIconName);

    [DllImport("user32.dll", EntryPoint = "GetClassLong")]
    private static extern uint GetClassLong32(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "GetClassLongPtr")]
    private static extern IntPtr GetClassLong64(IntPtr hWnd, int nIndex);

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      this.appList = new ListView();
      this.closeListView = new ListView();
      this.SuspendLayout();
      this.appList.Activation = ItemActivation.OneClick;
      this.appList.BackColor = Color.Black;
      this.appList.BorderStyle = BorderStyle.None;
      this.appList.Dock = DockStyle.Top;
      this.appList.Font = new Font("Microsoft Sans Serif", 7.8f, FontStyle.Bold, GraphicsUnit.Point, (byte) 0);
      this.appList.ForeColor = Color.White;
      this.appList.GridLines = true;
      this.appList.HoverSelection = true;
      this.appList.Location = new Point(6, 6);
      this.appList.Margin = new Padding(4);
      this.appList.MultiSelect = false;
      this.appList.Name = "appList";
      this.appList.Scrollable = false;
      this.appList.ShowGroups = false;
      this.appList.Size = new Size(82, 91);
      this.appList.TabIndex = 0;
      this.appList.UseCompatibleStateImageBehavior = false;
      this.closeListView.Activation = ItemActivation.OneClick;
      this.closeListView.BackColor = Color.Black;
      this.closeListView.BorderStyle = BorderStyle.None;
      this.closeListView.Dock = DockStyle.Fill;
      this.closeListView.ForeColor = Color.White;
      this.closeListView.GridLines = true;
      this.closeListView.HoverSelection = true;
      this.closeListView.Location = new Point(6, 97);
      this.closeListView.Margin = new Padding(4);
      this.closeListView.MultiSelect = false;
      this.closeListView.Name = "closeListView";
      this.closeListView.Scrollable = false;
      this.closeListView.ShowGroups = false;
      this.closeListView.Size = new Size(82, 27);
      this.closeListView.TabIndex = 1;
      this.closeListView.UseCompatibleStateImageBehavior = false;
      this.AutoScaleDimensions = new SizeF(120f, 120f);
      this.AutoScaleMode = AutoScaleMode.Dpi;
      this.BackColor = Color.Black;
      this.ClientSize = new Size(94, 130);
      this.Controls.Add((Control) this.closeListView);
      this.Controls.Add((Control) this.appList);
      this.FormBorderStyle = FormBorderStyle.None;
      this.Margin = new Padding(4);
      this.Name = nameof (WindowChooser);
      this.Opacity = 0.8;
      this.Padding = new Padding(6);
      this.StartPosition = FormStartPosition.Manual;
      this.Text = nameof (WindowChooser);
      this.TopMost = true;
      this.ResumeLayout(false);
    }
  }
}
