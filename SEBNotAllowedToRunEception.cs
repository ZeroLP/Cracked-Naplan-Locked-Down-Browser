
using System;

namespace SebWindowsClient
{
  public class SEBNotAllowedToRunEception : Exception
  {
    public SEBNotAllowedToRunEception(string message)
      : base(message)
    {
    }
  }
}
