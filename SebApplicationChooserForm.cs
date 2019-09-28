
using SebWindowsClient.ConfigurationUtils;
using SebWindowsClient.DiagnosticsUtils;
using SebWindowsClient.ProcessUtils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SebWindowsClient
{
  public class SebApplicationChooserForm : Form
  {
    private static uint WM_GETICON = (uint) sbyte.MaxValue;
    private static IntPtr ICON_SMALL2 = new IntPtr(2);
    private static IntPtr IDI_APPLICATION = new IntPtr(32512);
    private static int GCL_HICON = -14;
    private static int appChooserFormXPadding = 22;
    private static int appChooserFormXGap = 45;
    private List<IntPtr> lWindowHandles = new List<IntPtr>();
    private List<string> lWindowTitles = new List<string>();
    private int selectedItemIndex;
    private IContainer components;
    private ListView listApplications;

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, SebApplicationChooserForm.WindowShowStyle nCmdShow);

    [DllImport("User32.dll")]
    private static extern IntPtr SetForegroundWindow(IntPtr hWnd);

    [DllImport("User32.dll")]
    private static extern IntPtr SetActiveWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern IntPtr LoadIcon(IntPtr hInstance, IntPtr lpIconName);

    [DllImport("user32.dll", EntryPoint = "GetClassLong")]
    private static extern uint GetClassLong32(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "GetClassLongPtr")]
    private static extern IntPtr GetClassLong64(IntPtr hWnd, int nIndex);

    [DllImport("User32.DLL")]
    private static extern int AttachThreadInput(uint CurrentForegroundThread, uint MakeThisThreadForegrouond, bool boolAttach);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);

    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();

    [DllImport("kernel32.dll")]
    public static extern uint GetCurrentThreadId();

    [DllImport("user32.dll")]
    public static extern bool BringWindowToTop([In] IntPtr hWnd);

    public SebApplicationChooserForm()
    {
      this.InitializeComponent();
    }

    private static IntPtr GetClassLongPtr(IntPtr hWnd, int nIndex)
    {
      if (IntPtr.Size == 4)
        return new IntPtr((long) SebApplicationChooserForm.GetClassLong32(hWnd, nIndex));
      return SebApplicationChooserForm.GetClassLong64(hWnd, nIndex);
    }

    public static Image GetSmallWindowIcon(IntPtr hWnd)
    {
      try
      {
        try
        {
          return (Image) Icon.ExtractAssociatedIcon(hWnd.GetProcess().Modules[0].FileName).ToBitmap();
        }
        catch
        {
        }
        IntPtr num = new IntPtr();
        IntPtr handle = SebApplicationChooserForm.SendMessage(hWnd, SebApplicationChooserForm.WM_GETICON, SebApplicationChooserForm.ICON_SMALL2, IntPtr.Zero);
        if (handle == IntPtr.Zero)
          handle = SebApplicationChooserForm.GetClassLongPtr(hWnd, SebApplicationChooserForm.GCL_HICON);
        if (handle == IntPtr.Zero)
          handle = SebApplicationChooserForm.LoadIcon(IntPtr.Zero, (IntPtr) 32512);
        if (handle != IntPtr.Zero)
          return (Image) new Bitmap((Image) Icon.FromHandle(handle).ToBitmap(), 32, 32);
        return (Image) null;
      }
      catch (Exception ex)
      {
        return (Image) null;
      }
    }

    public void fillListApplications()
    {
      List<string> source = new List<string>();
      ImageList imageList = new ImageList();
      int num1 = (int) (32.0 * (double) SEBClientInfo.scaleFactor);
      if ((bool) SEBClientInfo.getSebSetting("touchOptimized")["touchOptimized"])
        num1 = (int) (48.0 * (double) SEBClientInfo.scaleFactor);
      imageList.ImageSize = new Size(num1, num1);
      imageList.ColorDepth = ColorDepth.Depth32Bit;
      this.lWindowHandles.Clear();
      int num2 = 0;
      foreach (KeyValuePair<IntPtr, string> openWindow in (IEnumerable<KeyValuePair<IntPtr, string>>) SEBWindowHandler.GetOpenWindows())
      {
        if (openWindow.Key.IsAllowed())
        {
          this.lWindowHandles.Add(openWindow.Key);
          this.lWindowTitles.Add(openWindow.Value);
          source.Add(openWindow.Value);
          if (openWindow.Key.GetProcess().ProcessName.Contains("xulrunner"))
          {
            Bitmap bitmap = Icon.ExtractAssociatedIcon(Application.ExecutablePath).ToBitmap();
            imageList.Images.Add((Image) bitmap);
          }
          else
          {
            Image smallWindowIcon = SebApplicationChooserForm.GetSmallWindowIcon(openWindow.Key);
            imageList.Images.Add("rAppIcon" + (object) num2, smallWindowIcon);
          }
          ++num2;
        }
      }
      this.listApplications.BeginUpdate();
      this.listApplications.Clear();
      this.listApplications.View = View.LargeIcon;
      this.listApplications.LargeImageList = imageList;
      this.listApplications.Scrollable = false;
      int num3 = 0;
      int num4 = 0;
      for (int index = 0; index < source.Count<string>(); ++index)
      {
        this.listApplications.Items.Add(new ListViewItem(source[index])
        {
          ImageIndex = index
        });
        Rectangle itemRect = this.listApplications.GetItemRect(index);
        Logger.AddInformation("ListView.GetItemRect: " + itemRect.ToString(), (object) null, (Exception) null, (string) null);
        num3 += itemRect.Width;
        if (itemRect.Height > num4)
          num4 = itemRect.Height;
      }
      this.listApplications.Dock = DockStyle.Fill;
      this.listApplications.AutoSize = true;
      int num5 = source.Count<string>();
      float scaleFactor = SEBClientInfo.scaleFactor;
      int num6 = 0;
      int num7 = num5 <= num6 ? (int) Math.Round((double) (2 * SebApplicationChooserForm.appChooserFormXPadding) * (double) scaleFactor) : (int) Math.Round((double) (2 * SebApplicationChooserForm.appChooserFormXPadding) * (double) scaleFactor + (double) num3);
      Rectangle bounds = Screen.PrimaryScreen.Bounds;
      if (bounds.Width < num7)
      {
        bounds = Screen.PrimaryScreen.Bounds;
        int width = bounds.Width;
        this.Height = (int) Math.Round((double) (2 * SebApplicationChooserForm.appChooserFormXPadding + 2 * num4 + SebApplicationChooserForm.appChooserFormXPadding) * (double) scaleFactor);
      }
      else
      {
        this.Width = num7;
        this.Height = (int) Math.Round((double) (2 * SebApplicationChooserForm.appChooserFormXPadding) * (double) scaleFactor + (double) num4);
      }
      this.listApplications.Height = this.Height - 24;
      this.CenterToScreen();
      this.listApplications.EndUpdate();
      if (this.listApplications.Items.Count == 1)
        this.selectedItemIndex = 0;
      if (this.listApplications.Items.Count > 1)
        this.selectedItemIndex = 1;
      this.SelectNextListItem();
    }

    public void SelectNextListItem()
    {
      if (this.listApplications.Items.Count <= 0)
        return;
      if (this.selectedItemIndex >= this.listApplications.Items.Count)
        this.selectedItemIndex = 0;
      this.listApplications.Items[this.selectedItemIndex].Selected = true;
      this.listApplications.Items[this.selectedItemIndex].Focused = true;
      string text = this.listApplications.Items[this.selectedItemIndex].Text;
      uint windowThreadProcessId = SebApplicationChooserForm.GetWindowThreadProcessId(SebApplicationChooserForm.GetForegroundWindow(), IntPtr.Zero);
      uint currentThreadId = SebApplicationChooserForm.GetCurrentThreadId();
      SebApplicationChooserForm.AttachThreadInput(windowThreadProcessId, currentThreadId, true);
      IntPtr lWindowHandle = this.lWindowHandles[this.selectedItemIndex];
      if (lWindowHandle == IntPtr.Zero)
      {
        foreach (IntPtr windowHandle in SEBWindowHandler.GetWindowHandlesByTitle(this.lWindowTitles[this.selectedItemIndex]))
          windowHandle.BringToTop(true);
      }
      else
        lWindowHandle.BringToTop(true);
      SebApplicationChooserForm.AttachThreadInput(windowThreadProcessId, currentThreadId, false);
      this.selectedItemIndex = this.selectedItemIndex + 1;
      this.listApplications.Focus();
    }

    public static void forceSetForegroundWindow(IntPtr hWnd)
    {
      int windowThreadProcessId1 = (int) SebApplicationChooserForm.GetWindowThreadProcessId(SebApplicationChooserForm.GetForegroundWindow(), IntPtr.Zero);
      uint currentThreadId = SebApplicationChooserForm.GetCurrentThreadId();
      int windowThreadProcessId2 = (int) SebApplicationChooserForm.GetWindowThreadProcessId(hWnd, IntPtr.Zero);
      int num1 = (int) currentThreadId;
      int num2 = 1;
      SebApplicationChooserForm.AttachThreadInput((uint) windowThreadProcessId1, (uint) num1, num2 != 0);
      SebApplicationChooserForm.SetForegroundWindow(hWnd);
      int num3 = (int) currentThreadId;
      int num4 = 0;
      SebApplicationChooserForm.AttachThreadInput((uint) windowThreadProcessId1, (uint) num3, num4 != 0);
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      this.listApplications = new ListView();
      this.SuspendLayout();
      this.listApplications.Activation = ItemActivation.OneClick;
      this.listApplications.BackColor = SystemColors.ControlLight;
      this.listApplications.BorderStyle = BorderStyle.None;
      this.listApplications.Dock = DockStyle.Fill;
      this.listApplications.GridLines = true;
      this.listApplications.HoverSelection = true;
      this.listApplications.Location = new Point(18, 17);
      this.listApplications.MultiSelect = false;
      this.listApplications.Name = "listApplications";
      this.listApplications.Size = new Size(66, 108);
      this.listApplications.TabIndex = 0;
      this.listApplications.UseCompatibleStateImageBehavior = false;
      this.AutoScaleDimensions = new SizeF(6f, 13f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.AutoSize = true;
      this.BackColor = SystemColors.ControlLight;
      this.ClientSize = new Size(94, 130);
      this.ControlBox = false;
      this.Controls.Add((Control) this.listApplications);
      this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = nameof (SebApplicationChooserForm);
      this.Opacity = 0.9;
      this.Padding = new Padding(18, 17, 10, 5);
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.StartPosition = FormStartPosition.CenterScreen;
      this.TransparencyKey = Color.Transparent;
      this.ResumeLayout(false);
    }

    private enum WindowShowStyle : uint
    {
      Hide = 0,
      ShowNormal = 1,
      ShowMinimized = 2,
      Maximize = 3,
      ShowMaximized = 3,
      ShowNormalNoActivate = 4,
      Show = 5,
      Minimize = 6,
      ShowMinNoActivate = 7,
      ShowNoActivate = 8,
      Restore = 9,
      ShowDefault = 10,
      ForceMinimized = 11,
    }
  }
}
