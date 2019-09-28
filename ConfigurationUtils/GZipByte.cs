
using System;
using System.IO;
using System.IO.Compression;

namespace SebWindowsClient.ConfigurationUtils
{
  public static class GZipByte
  {
    public static byte[] Compress(byte[] input)
    {
      using (MemoryStream memoryStream = new MemoryStream())
      {
        using (GZipStream gzipStream = new GZipStream((Stream) memoryStream, CompressionMode.Compress))
          gzipStream.Write(input, 0, input.Length);
        return memoryStream.ToArray();
      }
    }

    public static byte[] Decompress(byte[] input)
    {
      try
      {
        using (GZipStream gzipStream = new GZipStream((Stream) new MemoryStream(input), CompressionMode.Decompress))
        {
          byte[] buffer = new byte[4096];
          using (MemoryStream memoryStream = new MemoryStream())
          {
            int count;
            do
            {
              count = gzipStream.Read(buffer, 0, 4096);
              if (count > 0)
                memoryStream.Write(buffer, 0, count);
            }
            while (count > 0);
            return memoryStream.ToArray();
          }
        }
      }
      catch (Exception ex)
      {
        return (byte[]) null;
      }
    }
  }
}
