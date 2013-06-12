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

    private Boolean isEditing = false;
    private int minValue = 0;
    private int maxValue = 100;

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
      if (btnEdit.Content == "Edit")
      {
        btnEdit.Content = "Save";
        tpKeyboard.Visibility = System.Windows.Visibility.Visible;
      }
      else
      {
        btnEdit.Content = "Edit";
        tpKeyboard.Visibility = System.Windows.Visibility.Collapsed;
      }
    }

    public void tpKeyboard_EnterKeyPressed(System.Object sender, EventArgs args)
    {

    }

    public void tpKeyboard_AlphaNumericKeyPressed(System.Object sender, EventArgs args)
    {

    }
  }
}
