using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace UofM.HCI.tPad.App.WhatUp
{
  public class AlignmentConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      String localID = values[0].ToString();
      String local = String.Format("Device-{0}", localID);

      String from = (String)values[1];

      if (from == local)
        return System.Windows.HorizontalAlignment.Right;
      return System.Windows.HorizontalAlignment.Left;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
