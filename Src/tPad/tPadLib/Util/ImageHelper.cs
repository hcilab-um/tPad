using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Drawing;
using System.IO;

namespace UofM.HCI.tPab.Util
{
  class ImageHelper
  {
    //public static System.Windows.Media.Imaging.BitmapSource Crop(System.Windows.Media.Imaging.BitmapSource source, System.Windows.Point offset, Rect bounds, float angle)
    //{
    //  Bitmap bmpImage = CreateBitmapFromBitmapSource(source);
    //  Bitmap bmpCrop = bmpImage.Clone(new Rectangle((int)offset.X, (int)offset.Y, (int)bounds.Width, (int)bounds.Height), bmpImage.PixelFormat);
    //  return CreateBitmapSourceFromBitmap(bmpCrop);
    //}

    //public static Bitmap CreateBitmapFromBitmapSource(System.Windows.Media.Imaging.BitmapSource bitmapsource)
    //{
    //  Bitmap bitmap;
    //  using (MemoryStream stream = new MemoryStream())
    //  {
    //    // from System.Media.BitmapImage to System.Drawing.Bitmap 
    //    System.Windows.Media.Imaging.PngBitmapEncoder enc = new System.Windows.Media.Imaging.PngBitmapEncoder();
    //    enc.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bitmapsource));
    //    enc.Save(stream);
    //    bitmap = new System.Drawing.Bitmap(stream);
    //  }
    //  return bitmap;
    //}

    //public static System.Windows.Media.Imaging.BitmapSource CreateBitmapSourceFromBitmap(Bitmap bitmap)
    //{
    //  if (bitmap == null)
    //    throw new ArgumentNullException("bitmap");

    //  if (Application.Current.Dispatcher == null)
    //    return null; // Is it possible?

    //  try
    //  {
    //    using (MemoryStream memoryStream = new MemoryStream())
    //    {
    //      // You need to specify the image format to fill the stream. 
    //      // I'm assuming it is PNG
    //      bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
    //      memoryStream.Seek(0, SeekOrigin.Begin);

    //      // Make sure to create the bitmap in the UI thread
    //      if (InvokeRequired)
    //        return (System.Windows.Media.Imaging.BitmapSource)Application.Current.Dispatcher.Invoke(
    //            new Func<Stream, System.Windows.Media.Imaging.BitmapSource>(CreateBitmapSourceFromBitmap),
    //            System.Windows.Threading.DispatcherPriority.Normal,
    //            memoryStream);

    //      return CreateBitmapSourceFromBitmap(memoryStream);
    //    }
    //  }
    //  catch (Exception)
    //  {
    //    return null;
    //  }
    //}

    //private static bool InvokeRequired
    //{
    //  get { return System.Windows.Threading.Dispatcher.CurrentDispatcher != Application.Current.Dispatcher; }
    //}

    //private static System.Windows.Media.Imaging.BitmapSource CreateBitmapSourceFromBitmap(Stream stream)
    //{
    //  System.Windows.Media.Imaging.BitmapDecoder bitmapDecoder = System.Windows.Media.Imaging.BitmapDecoder.Create(
    //      stream,
    //      System.Windows.Media.Imaging.BitmapCreateOptions.PreservePixelFormat,
    //      System.Windows.Media.Imaging.BitmapCacheOption.OnLoad);

    //  // This will disconnect the stream from the image completely...
    //  System.Windows.Media.Imaging.WriteableBitmap writable = new System.Windows.Media.Imaging.WriteableBitmap(bitmapDecoder.Frames.Single());
    //  writable.Freeze();

    //  return writable;
    //}

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
