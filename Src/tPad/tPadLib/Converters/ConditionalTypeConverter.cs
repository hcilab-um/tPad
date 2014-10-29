//  Public domain converter that allows you to output objects of an arbitrary type, 
//  based on the value of some other property. Helpful when you want to output, for 
//  example, "Opacity" as 1.0 or 0.25, based on an IsEnabled bool property.
//
//  Usage Example:
//
//  <Button Opacity="{Bind Path=IsEnabled, ElementName=theButton, ValueConverter={local:ConditionalValueConverter Reference=true TrueValue=1.0 FalseValue=0.25 ValueType=double}}"
//        Name="theButton">Text</Button>
//
//  This converter can return Brushes, Bitmaps, or whatever else you want, based on 
//  a comparison with a reference value. You might want to extend it to support 
//  comparisons other than equals (such as smaller, greater, etc), but it's already 
//  quite useful, reducing code bloat in my WPF projects.
//
//  Released into the public domain by Jon Watte, February 8, 2009

using System;
using System.ComponentModel;
using System.Windows.Data;

namespace UofM.HCI.tPad.Converters
{

  [ValueConversion(typeof(object), typeof(object))]
  public class ConditionalValueConverter : IValueConverter
  {

    public ConditionalValueConverter()
    {
    }

    public string Reference { get; set; }

    object trueValue_;
    object setTrueValue_;
    public object TrueValue
    {
      get { return trueValue_; }
      set
      {
        setTrueValue_ = value;
        MakeTrue();
      }
    }

    object falseValue_;
    object setFalseValue_;
    public object FalseValue
    {
      get { return falseValue_; }
      set
      {
        setFalseValue_ = value;
        MakeFalse();
      }
    }

    Type valueType_;
    public string ValueType
    {
      get { return valueType_.Name; }
      set
      {
        valueType_ = GetValueType(value);
        MakeTrue();
        MakeFalse();
      }
    }

    Type GetValueType(string name)
    {
      if (name == "float" || name == "System.Single")
        return typeof(float);
      if (name == "double" || name == "System.Double")
        return typeof(double);
      if (name == "int" || name == "System.Int32")
        return typeof(int);
      if (name == "string" || name == "System.String")
        return typeof(string);
      if (name == "bool" || name == "System.Boolean")
        return typeof(bool);
      if (name == "System.Windows.Visibility")
        return typeof(System.Windows.Visibility);
      if (name == "bool?")
        return typeof(bool?);
      return Type.GetType(name);
    }

    void MakeTrue()
    {
      if (setTrueValue_ == null || valueType_ == null)
        return;

      if (setTrueValue_.GetType() == valueType_)
      {
        trueValue_ = setTrueValue_;
        return;
      }

      if (setTrueValue_.GetType() != typeof(string))
      {
        throw new InvalidOperationException(
            String.Format("Set type must be ValueType ({0}) or string for ConditionalValueConverter.TrueValue. Got type {1}.",
            valueType_.Name, setTrueValue_.GetType().Name));
      }

      trueValue_ = TypeDescriptor.GetConverter(valueType_).ConvertFromInvariantString((string)setTrueValue_);
    }

    void MakeFalse()
    {
      if (setFalseValue_ == null || valueType_ == null)
        return;

      if (setFalseValue_.GetType() == valueType_)
      {
        falseValue_ = setFalseValue_;
        return;
      }

      if (setFalseValue_.GetType() != typeof(string))
      {
        throw new InvalidOperationException(
            String.Format("Set type must be ValueType ({0}) or string for ConditionalValueConverter.FalseValue. Got type {1}.",
            valueType_.Name, setFalseValue_.GetType().Name));
      }

      falseValue_ = TypeDescriptor.GetConverter(valueType_).ConvertFromInvariantString((string)setFalseValue_);
    }

    #region IValueConverter Members

    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      if (targetType != valueType_)
        throw new System.NotSupportedException();

      if (value == null || Reference == null)
        return ((object)value == (object)Reference) ? trueValue_ : falseValue_;

      object r = Reference;
      if (value.GetType() != Reference.GetType())
        r = TypeDescriptor.GetConverter(value).ConvertFrom(r);

      if (value.Equals(r))
        return trueValue_;
      return falseValue_;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      if (!targetType.IsEnum)
        return null;

      bool isChecked = (bool)value;
      if (isChecked)
        return Enum.Parse(targetType, Reference);
      else
        return Enum.GetValues(targetType).GetValue(0);
    }

    #endregion
  }
}
