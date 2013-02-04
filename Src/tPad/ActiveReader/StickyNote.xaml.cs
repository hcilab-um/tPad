using System;
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

namespace UofM.HCI.tPab.App.ActiveReader
{
  /// <summary>
  /// Interaction logic for StickyNote.xaml
  /// </summary>
  public partial class StickyNote : UserControl
  {
    public StickyNote(double marginLeft, double marginRight)
    {
      this.Margin = new Thickness(marginLeft, marginRight, 0, 0);
      InitializeComponent();
    }

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

    public TextBox TextField
    {
      get { return tNote; }
      set
      {
        tNote = value;
        OnPropertyChanged("TextField");
      }
    }

    public Button BClose { get { return bClose; } }
    public Image BResize { get { return bResize; } }
    public Grid GNote { get { return gNote; } }
   
    public event PropertyChangedEventHandler PropertyChanged;
    public void OnPropertyChanged(String name)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(name));
    }
    
    private Size defaultNoteSize = new Size(30, 30);
    private void bResize_MouseDown(object sender, MouseButtonEventArgs e)
    {
      if (e.LeftButton == MouseButtonState.Pressed && e.RightButton == MouseButtonState.Released)
        IsBResizeClicked = true;
    }

    private void bResize_MouseUp(object sender, MouseButtonEventArgs e)
    {
      IsBResizeClicked = false;
    }
  }
}
