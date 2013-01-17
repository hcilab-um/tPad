using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Drawing;
using System.IO;
using System.Threading;

namespace UofM.HCI.tPab.Util
{
  class ImageHelper
  {

    /// <summary>
    /// Partly taken from: http://www.abhisheksur.com/2010/04/screen-capture-using-wpf-winforms.html
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    public static Bitmap ScreenCapture(double x, double y, Rect bounds)
    {
      int ix, iy, iw, ih;
      ix = Convert.ToInt32(x);
      iy = Convert.ToInt32(y);
      iw = Convert.ToInt32(bounds.Width);
      ih = Convert.ToInt32(bounds.Height);
      Bitmap image = new Bitmap(iw, ih, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
      Graphics g = Graphics.FromImage(image);

      g.CopyFromScreen(ix, iy, 0, 0, new System.Drawing.Size(iw, ih), CopyPixelOperation.SourceCopy);

      return image;
    }

    public static Bitmap RotateImageByAngle(System.Drawing.Image oldBitmap, float angle, Rect bounds)
    {
      if (angle == 0)
        return oldBitmap as Bitmap;

      var newBitmap = new Bitmap((int)bounds.Width, (int)bounds.Height);
      var graphics = Graphics.FromImage(newBitmap);
      graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
      graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
      graphics.TranslateTransform((float)bounds.Width / 2, (float)bounds.Height / 2);
      graphics.RotateTransform(angle);
      graphics.TranslateTransform(-(float)bounds.Width / 2, -(float)bounds.Height / 2);

      int widthDiff = ((int)bounds.Width - oldBitmap.Width) / 2;
      int heightDiff = ((int)bounds.Height - oldBitmap.Height) / 2;
      graphics.DrawImage(oldBitmap, new PointF(widthDiff, heightDiff));
      return newBitmap;
    }

  }
}
