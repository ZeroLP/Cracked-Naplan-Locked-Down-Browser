

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SebWindowsClient.ServiceUtils
{
  internal static class InputParamsManager
  {
    public static InputParamsManager.Parameters Get()
    {
      InputParamsManager.Parameters parameters = new InputParamsManager.Parameters();
      int? nullable1 = InputParamsManager.ReadRegKeyValue<int?>(InputParamsManager.OpenLatestTabTipSubKey(true), "EnableTextPrediction", new int?());
      int? nullable2 = nullable1;
      int num1 = 1;
      bool? nullable3;
      if ((nullable2.GetValueOrDefault() == num1 ? (nullable2.HasValue ? 1 : 0) : 0) == 0)
      {
        nullable2 = nullable1;
        int num2 = 0;
        nullable3 = (nullable2.GetValueOrDefault() == num2 ? (nullable2.HasValue ? 1 : 0) : 0) != 0 ? new bool?(false) : new bool?();
      }
      else
        nullable3 = new bool?(true);
      parameters.IsPredictionEnabled = nullable3;
      return parameters;
    }

    public static void Set(InputParamsManager.Parameters parameters)
    {
      if (parameters == null)
        throw new ArgumentNullException(nameof (parameters));
      if (!parameters.IsPredictionEnabled.HasValue)
        return;
      InputParamsManager.WriteRegKeyValue<int>(InputParamsManager.OpenLatestTabTipSubKey(true), "EnableTextPrediction", parameters.IsPredictionEnabled.Value ? 1 : 0);
    }

    private static RegistryKey OpenLatestTabTipSubKey(bool isReadonly = true)
    {
      RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\TabletTip");
      if (registryKey == null)
        return (RegistryKey) null;
      var data = ((IEnumerable<string>) registryKey.GetSubKeyNames()).Select(k =>
      {
        Version result;
        if (!Version.TryParse(k, out result))
          return null;
        return new{ Key = k, Version = result };
      }).Where(i => i != null).OrderByDescending(i => i.Version).FirstOrDefault();
      string name = data != null ? data.Key : (string) null;
      if (string.IsNullOrEmpty(name))
        return (RegistryKey) null;
      return registryKey.OpenSubKey(name, isReadonly);
    }

    private static T ReadRegKeyValue<T>(RegistryKey key, string valueName, T fallback = default(T))
    {
      try
      {
        return (T) key.GetValue(valueName, (object) fallback);
      }
      catch
      {
        return fallback;
      }
    }

    private static void WriteRegKeyValue<T>(RegistryKey key, string valueName, T value)
    {
      try
      {
        key.SetValue(valueName, (object) value);
      }
      catch
      {
      }
    }

    public class Parameters : ICloneable
    {
      public bool? IsPredictionEnabled { get; set; }

      public object Clone()
      {
        return (object) new InputParamsManager.Parameters()
        {
          IsPredictionEnabled = this.IsPredictionEnabled
        };
      }
    }
  }
}
