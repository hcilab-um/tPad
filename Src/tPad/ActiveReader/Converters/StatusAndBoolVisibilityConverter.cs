using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace UofM.HCI.tPab.App.ActiveReader.Converters
{
  public class StatusAndBoolVisibilityConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      if (values == null || values.Length != 4 || values[0] == DependencyProperty.UnsetValue)
        return System.Windows.Visibility.Visible;

      String[] parameters = (parameter as String).Split(',');

      LocationStatus status = (LocationStatus)values[0];
      LocationStatus targetStatus = (LocationStatus)Enum.Parse(typeof(LocationStatus), parameters[0]);

      bool gate1 = (bool)values[1];
      bool targetGate1 = Boolean.Parse(parameters[1]);

      bool gate2 = (bool)values[2];
      bool targetGate2 = Boolean.Parse(parameters[2]);

      bool gate3 = (bool)values[3];
      bool targetGate3 = Boolean.Parse(parameters[3]);

      if (status == targetStatus && gate1 == targetGate1 && (gate2 == targetGate2 || gate3 == targetGate3))
        return System.Windows.Visibility.Visible;
      return System.Windows.Visibility.Collapsed;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
