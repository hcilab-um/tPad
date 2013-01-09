﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace UofM.HCI.tPab.App.ActiveReader.Converters
{
  public class StatusAndModeVisibilityConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      if (values == null || values.Length != 2 || values[0] == DependencyProperty.UnsetValue || values[1] == DependencyProperty.UnsetValue)
        return System.Windows.Visibility.Visible;

      String[] parameters = (parameter as String).Split(',');

      LocationStatus status = (LocationStatus)values[0];
      LocationStatus targetStatus = (LocationStatus)Enum.Parse(typeof(LocationStatus), (String)parameters[0]);

      ActiveReaderApp.ActiveReaderMode mode = (ActiveReaderApp.ActiveReaderMode)values[1];
      ActiveReaderApp.ActiveReaderMode targetMode = (ActiveReaderApp.ActiveReaderMode)Enum.Parse(typeof(ActiveReaderApp.ActiveReaderMode), (String)parameters[1]);

      if (status == targetStatus && mode == targetMode)
        return System.Windows.Visibility.Visible;
      return System.Windows.Visibility.Collapsed;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
