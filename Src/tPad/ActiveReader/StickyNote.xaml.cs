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
using System.Xml.Serialization;

namespace UofM.HCI.tPab.App.ActiveReader
{
  /// <summary>
  /// Interaction logic for StickyNote.xaml
  /// </summary>
  public partial class StickyNote : UserControl, INotifyPropertyChanged
  {

    private bool isBResizeClicked = false;
    public bool IsBResizeClicked
    {
      get { return isBResizeClicked; }
      set
      {
        isBResizeClicked = value;
        OnPropertyChanged("IsBResizeClicked");
      }
    }

    private bool isNoteMoving = false;
    public bool IsNoteMoving
    {
      get { return isNoteMoving; }
      set
      {
        isNoteMoving = value;
        OnPropertyChanged("IsNoteMoving");
      }
    }

    private double widthFactor = 0;
    public double WidthFactor
    {
      get { return widthFactor; }
      set
      {
        widthFactor = value;
        OnPropertyChanged("WidthFactor");
        OnPropertyChanged("InvWidthFactor");
      }
    }

    public double InvWidthFactor
    {
      get
      {
        if (WidthFactor != 0)
          return 1 / WidthFactor;
        return 0;
      }
    }

    private double heightFactor = 0;
    public double HeightFactor
    {
      get { return heightFactor; }
      set
      {
        heightFactor = value;
        OnPropertyChanged("HeightFactor");
        OnPropertyChanged("InvHeightFactor");
      }
    }

    public double InvHeightFactor
    {
      get
      {
        if (HeightFactor != 0)
          return 1 / HeightFactor;
        return 0;
      }
    }

    public StickyNote(double marginLeft, double marginRight)
    {
      this.Margin = new Thickness(marginLeft, marginRight, 0, 0);
      InitializeComponent();
    }

    private void stickyNote_Loaded(object sender, RoutedEventArgs e)
    {

    }

    public event PropertyChangedEventHandler PropertyChanged;
    public void OnPropertyChanged(String name)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(name));
    }

    public TextBox getTextField() { return tNote; }

    public Button getBClose() { return bClose; }

    public Image getBResize() { return bResize; }

    public Grid getGNote() { return gNote; }

  }

}