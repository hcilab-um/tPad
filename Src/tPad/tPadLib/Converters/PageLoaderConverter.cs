using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.IO;
using UofM.HCI.tPab.Properties;
using System.Windows;
using UofM.HCI.tPab.Services;

namespace UofM.HCI.tPab.Converters
{
  public class PageLoaderConverter : IMultiValueConverter
  {

    private static log4net.ILog logger = log4net.LogManager.GetLogger(typeof(PageLoaderConverter));
    private static BitmapImage sampleDoc = null;

    public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      if (values.Length != 2 || values[1] == null || values[0] == DependencyProperty.UnsetValue)
        return GetSampleDoc();

      int actualPage = (int)values[0];
      TPadDocument actualDocument = (TPadDocument)values[1];

      if (actualPage == -1)
        return GetSampleDoc();

      String pageFileName = actualDocument.Pages[actualPage].FileName;
      if (File.Exists(pageFileName))
      {
        try
        {
          Uri uri = new Uri(pageFileName, UriKind.RelativeOrAbsolute);
          BitmapFrame source = BitmapFrame.Create(uri);
          return source;
        }
        catch (Exception e)
        {
          logger.Error(String.Format("Error loading user image {0} - Detail: {1}", pageFileName, e.Message));
          return GetSampleDoc();
        }
      }

      throw new NotImplementedException();
    }

    private object GetSampleDoc()
    {
      if (sampleDoc != null)
        return sampleDoc;

      MemoryStream stream = new MemoryStream();
      Resources.SampleDoc.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);
      stream.Seek(0, SeekOrigin.Begin);

      sampleDoc = new BitmapImage();
      sampleDoc.BeginInit();
      sampleDoc.StreamSource = stream;
      sampleDoc.EndInit();

      return sampleDoc;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
