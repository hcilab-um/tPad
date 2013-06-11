using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace UofM.HCI.tPad.App.ActiveReader.Converters
{
  public class FullOrHalfPageConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      var size = (double)values[0];
      var isFullPage = (bool)values[1];
      var uiRotationAngle = (double)values[2];
      var targetVar = parameter as String;

      if (isFullPage)
        return size;

      if ((uiRotationAngle == 0 || uiRotationAngle == 180) && targetVar == "Height")
        return size / 2;

      if ((uiRotationAngle == 90 || uiRotationAngle == 270) && targetVar == "Width")
        return size / 2;

      return size;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
