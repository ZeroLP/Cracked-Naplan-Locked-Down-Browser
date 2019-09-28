// Decompiled with JetBrains decompiler
// Type: SebWindowsClient.NoSelectButton
// Assembly: SafeExamBrowser, Version=2.1.1.76, Culture=neutral, PublicKeyToken=null
// MVID: C83D2177-299E-4999-8CCB-0DBBE2549D1F
// Assembly location: C:\Program Files (x86)\NAP Locked down browser\SafeExamBrowser.exe

using System.Windows.Forms;

namespace SebWindowsClient
{
  internal class NoSelectButton : Button
  {
    public NoSelectButton()
    {
      this.SetStyle(ControlStyles.Selectable, false);
    }
  }
}
