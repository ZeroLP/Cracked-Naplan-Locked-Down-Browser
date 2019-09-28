// Decompiled with JetBrains decompiler
// Type: SebWindowsClient.TaskbarToolStrip
// Assembly: SafeExamBrowser, Version=2.1.1.76, Culture=neutral, PublicKeyToken=null
// MVID: C83D2177-299E-4999-8CCB-0DBBE2549D1F
// Assembly location: C:\Program Files (x86)\NAP Locked down browser\SafeExamBrowser.exe

using System.Windows.Forms;

namespace SebWindowsClient
{
  internal class TaskbarToolStrip : ToolStrip
  {
    protected override void WndProc(ref Message m)
    {
      if (m.Msg == 33 && this.CanFocus && !this.Focused)
        this.Focus();
      base.WndProc(ref m);
    }
  }
}
