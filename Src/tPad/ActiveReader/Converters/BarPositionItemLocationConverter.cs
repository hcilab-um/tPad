using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Controls;
using System.Windows;

namespace UofM.HCI.tPab.App.ActiveReader.Converters
{
  class BarPositionItemLocationConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      var pageHeight = double.Parse(values[0].ToString());
      var deviceLocationY = ((Point)values[1]).Y;
      var heightFactor = double.Parse(values[2].ToString());
      var itemHeight = double.Parse(values[3].ToString()) - double.Parse(values[4].ToString()); //subtract height of highlight element from height of page item height

      double relativePagePosition = ((deviceLocationY * heightFactor) / pageHeight) * itemHeight;

      return new System.Windows.Thickness(-8, (relativePagePosition), 0, 0);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
