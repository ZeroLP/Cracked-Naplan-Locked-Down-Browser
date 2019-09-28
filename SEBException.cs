
using System;

namespace SebWindowsClient
{
  public class SEBException : ApplicationException
  {
    public SEBException(string message, Exception innerException = null)
      : base(message, innerException)
    {
    }

    public override string Source
    {
      get
      {
        return "SEB";
      }
      set
      {
        base.Source = value;
      }
    }
  }
}
