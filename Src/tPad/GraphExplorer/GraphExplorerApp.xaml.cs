﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using UofM.HCI.tPad.Converters;

namespace UofM.HCI.tPad.App.GraphExplorer
{
  /// <summary>
  /// Interaction logic for UserControl1.xaml
  /// </summary>
  public partial class GraphExplorerApp : UserControl, ITPadApp, INotifyPropertyChanged
  {

    public event EventHandler Closed;
    public event PropertyChangedEventHandler PropertyChanged;

    public TPadCore Core { get; set; }
    public ITPadAppContainer Container { get; set; }
    public ITPadAppController Controller { get; set; }

    private bool isEditing = false;
    public bool IsEditing
    {
      get { return isEditing; }
      set
      {
        isEditing = value;
        OnPropertyChanged("IsEditing");
      }
    }

    private double minValue = 0;
    public double MinValue
    {
      get { return minValue; }
      set
      {
        minValue = value;
        OnPropertyChanged("MinValue");
      }
    }

    private double maxValue = 100;
    public double MaxValue
    {
      get { return maxValue; }
      set
      {
        maxValue = value;
        OnPropertyChanged("MaxValue");
      }
    }

    public GraphExplorerApp(TPadCore core, ITPadAppContainer container, ITPadAppController controller)
    {
      InitializeComponent();
    }

    public void Close()
    {
      if (Closed != null)
        Closed(this, EventArgs.Empty);
    }

    public void OnPropertyChanged(String name)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(name));
    }

    private void btnClose_Click(object sender, RoutedEventArgs e)
    {
      Close();
    }

    private void btnEdit_Click(object sender, RoutedEventArgs e)
    {
      if (!IsEditing)
      {
        IsEditing = true;
        btnEdit.Content = "Save";
        tpKeyboard.Visibility = System.Windows.Visibility.Visible;
      }
      else
      {
        IsEditing = false;
        btnEdit.Content = "Edit";
        tpKeyboard.Visibility = System.Windows.Visibility.Collapsed;
      }
    }

    public void tpKeyboard_EnterKeyPressed(System.Object sender, EventArgs args)
    {
      if (currentTB == null)
      {
        MessageBox.Show("Please select a value to change first (pink box).");
        return;
      }

      double value = 0;
      if (!Double.TryParse(tpKeyboard.CurrentTextLine.ToString(), out value))
      {
        MessageBox.Show("Only 'double' numbers.");
        return;
      }

      if (currentTB == tbMinValue && value < MaxValue)
        MinValue = value;
      else if (currentTB == tbMaxValue && value > MinValue)
        MaxValue = value;
      currentTB.Background = Brushes.White;
      currentTB = null;
    }

    public void tpKeyboard_AlphaNumericKeyPressed(System.Object sender, EventArgs args)
    {
      tpKeyboard.ShowNumericKeyboard = true;
    }

    private TextBox currentTB = null;
    private void TextBox_GotFocus(object sender, RoutedEventArgs e)
    {
      if (currentTB != null)
        currentTB.Background = Brushes.White;

      currentTB = (sender as TextBox);
      currentTB.Background = Brushes.LightPink;
      tpKeyboard.InitialText = currentTB.Text;
    }

    private void TextBox_LostFocus(object sender, RoutedEventArgs e)
    {
    }

    private void gExplorer_MouseMove(object sender, MouseEventArgs e)
    {
      if (IsEditing && isMoving)
      {
        Point position = Mouse.GetPosition(this);
        yAxis.Y2 = position.Y;
      }
    }

    private bool cancelMouseUp = false;
    void mark_MarkClicked(object sender, EventArgs e)
    {
      ValueMark mark = (sender as ValueMark);
      gExplorer.Children.Remove(mark);
      cancelMouseUp = true;
    }

    private double GetDistanceBetweenPoints(Point start, Point final)
    {
      double a = start.X - final.X;
      double b = start.Y - final.Y;
      double distance = Math.Sqrt(a * a + b * b);
      return distance;
    }

    private bool isMoving = false;
    private void gExplorer_MouseDown(object sender, MouseButtonEventArgs e)
    {
      isMoving = true;
    }

    private void gExplorer_MouseUp(object sender, MouseButtonEventArgs e)
    {
      isMoving = false;
      if (IsEditing)
        return;
      if (cancelMouseUp)
      {
        cancelMouseUp = false;
        return;
      }

      double distanceInPixels = GetDistanceBetweenPoints(new Point(yAxis.X1, yAxis.Y1), new Point(yAxis.X2, yAxis.Y2));
      double chartRange = MaxValue - MinValue;
      double ratio = distanceInPixels / chartRange;

      Point queryPos = Mouse.GetPosition(this);
      double queryY = queryPos.Y;
      double queryChartValue = (yAxis.Y2 - queryY) / ratio;

      ValueMark mark = new ValueMark() { Mark = queryChartValue };
      mark.Margin = new Thickness(queryPos.X, queryPos.Y, 0, 0);
      mark.VerticalAlignment = System.Windows.VerticalAlignment.Top;
      mark.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
      mark.MarkClicked += new EventHandler(mark_MarkClicked);

      TranslateTransform transform = new TranslateTransform();
      MultiplierConverter converter = new MultiplierConverter();
      BindingOperations.SetBinding(transform, TranslateTransform.XProperty, new Binding("ActualWidth") { Source = mark, Converter = converter, ConverterParameter = -0.5 });
      BindingOperations.SetBinding(transform, TranslateTransform.YProperty, new Binding("ActualHeight") { Source = mark, Converter = converter, ConverterParameter = -1 });
      mark.RenderTransform = transform;
      
      gExplorer.Children.Add(mark);
    }
  }
}