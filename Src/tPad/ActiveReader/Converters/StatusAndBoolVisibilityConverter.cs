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
      if (values == null || values.Length != 3 || values[0] == DependencyProperty.UnsetValue || values[1] == DependencyProperty.UnsetValue)
        return System.Windows.Visibility.Visible;

      String[] parameters = (parameter as String).Split(',');

      LocationStatus status = (LocationStatus)values[0];
      LocationStatus targetStatus = (LocationStatus)Enum.Parse(typeof(LocationStatus), parameters[0]);

      bool gate = (bool)values[1];
      bool targetGate = Boolean.Parse(parameters[1]);

      bool gate1 = (bool)values[2];
      bool targetGate1 = Boolean.Parse(parameters[2]);

      if (status == targetStatus && (gate == targetGate || gate1 == targetGate1))
        return System.Windows.Visibility.Visible;
      return System.Windows.Visibility.Collapsed;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
