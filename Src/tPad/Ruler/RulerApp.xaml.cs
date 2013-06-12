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

namespace UofM.HCI.tPad.App.Ruler
{
  /// <summary>
  /// Interaction logic for RulerApp.xaml
  /// </summary>
  public partial class RulerApp : UserControl, ITPadApp, INotifyPropertyChanged
  {

    public event EventHandler Closed;
    public event PropertyChangedEventHandler PropertyChanged;

    public TPadCore Core { get; set; }
    public ITPadAppContainer Container { get; set; }
    public ITPadAppController Controller { get; set; }

    private double distance = 0.0;
    public double Distance
    {
      get { return distance; }
      set
      {
        distance = value;
        OnPropertyChanged("Distance");
      }
    }

    public RulerApp(TPadCore core, ITPadAppContainer container, ITPadAppController controller)
    {
      Core = core;
      Container = container;
      Controller = controller;

      InitializeComponent();
      Distance = GetDistanceBetweenPoints(new Point(measureLine.X1, measureLine.Y1), new Point(measureLine.X2, measureLine.Y2));
    }

    private void btnClose_Click(object sender, RoutedEventArgs e)
    {
      Close();
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

    private bool isMoving = false, isHead = true;
    private void gMeasurements_MouseDown(object sender, MouseButtonEventArgs e)
    {
      Point location = Mouse.GetPosition(this);
      double distanceToHead = GetDistanceBetweenPoints(location, new Point(measureLine.X1, measureLine.Y1));
      double distanceToTail = GetDistanceBetweenPoints(location, new Point(measureLine.X2, measureLine.Y2));
      if (distanceToHead <= distanceToTail)
        isHead = true;
      else
        isHead = false;
      isMoving = true;

      UpdateLine(location, isHead);
    }

    private void gMeasurements_MouseMove(object sender, MouseEventArgs e)
    {
      if (!isMoving)
        return;

      Point location = Mouse.GetPosition(this);
      UpdateLine(location, isHead);
    }

    private void gMeasurements_MouseUp(object sender, MouseButtonEventArgs e)
    {
      isMoving = false;
    }

    private void UpdateLine(Point location, bool isHead)
    {
      if (isHead)
      {
        measureLine.X1 = location.X;
        measureLine.Y1 = location.Y;
      }
      else
      {
        measureLine.X2 = location.X;
        measureLine.Y2 = location.Y;
      }
      Distance = GetDistanceBetweenPoints(new Point(measureLine.X1, measureLine.Y1), new Point(measureLine.X2, measureLine.Y2));
    }

    private double GetDistanceBetweenPoints(Point start, Point final)
    {
      double a = start.X - final.X;
      double b = start.Y - final.Y;
      double distance = Math.Sqrt(a * a + b * b);
      return distance;
    }
  }

}