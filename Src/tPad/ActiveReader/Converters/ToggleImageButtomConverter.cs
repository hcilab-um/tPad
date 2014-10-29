using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.IO;

namespace UofM.HCI.tPad.App.ActiveReader.Converters
{
  public class ToggleImageButtomConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      var isChecked = (bool)values[0];
      var imageBase = (String)values[1];

      String imagePath = String.Format("Images/{0}.png", imageBase);
      if(isChecked)
        imagePath = String.Format("Images/{0}Selected.png", imageBase);

      FileStream pngStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read);
      PngBitmapDecoder pngDecoder = new PngBitmapDecoder(pngStream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
      BitmapFrame pngFrame = pngDecoder.Frames[0];
      return pngFrame;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
