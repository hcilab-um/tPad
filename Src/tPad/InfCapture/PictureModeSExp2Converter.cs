using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace UofM.HCI.tPad.App.InfCapture
{
  public class PictureModeSExp2Converter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      PictureMode actualD = (PictureMode)value;
      if (actualD == PictureMode.Clipped)
        return "Clip Picture";
      return "No Clipping";
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
