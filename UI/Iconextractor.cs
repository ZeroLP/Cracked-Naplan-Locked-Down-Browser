

using System;
using System.Drawing;
using System.Drawing.IconLib;
using System.Linq;

namespace SebWindowsClient.UI
{
  public static class Iconextractor
  {
    public static Bitmap ExtractHighResIconImage(string path, int? size = null)
    {
      MultiIcon source1 = new MultiIcon();
      string fileName = path;
      source1.Load(fileName);
      SingleIcon source2 = source1.FirstOrDefault<SingleIcon>();
      if (source2 != null)
      {
        if (size.HasValue)
        {
          if (size.Value <= 32)
          {
            try
            {
              return Icon.ExtractAssociatedIcon(path).ToBitmap();
            }
            catch
            {
            }
          }
          IconImage iconImage = source2.Where<IconImage>((Func<IconImage, bool>) (x => x.Size.Height >= size.Value)).OrderBy<IconImage, int>((Func<IconImage, int>) (x => x.Size.Height)).FirstOrDefault<IconImage>();
          if (iconImage != null)
            return iconImage.Icon.ToBitmap();
        }
        int max = source2.Max<IconImage>((Func<IconImage, int>) (_i => _i.Size.Height));
        IconImage iconImage1 = source2.FirstOrDefault<IconImage>((Func<IconImage, bool>) (i => i.Size.Height == max));
        if (iconImage1 != null)
          return iconImage1.Transparent;
      }
      return (Bitmap) null;
    }
  }
}
