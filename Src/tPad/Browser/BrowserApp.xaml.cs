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

namespace UofM.HCI.tPad.App.Browser
{
  /// <summary>
  /// Interaction logic for BrowserApp.xaml
  /// </summary>
  public partial class BrowserApp : UserControl, ITPadApp, INotifyPropertyChanged
  {

    public event EventHandler Closed;
    public event PropertyChangedEventHandler PropertyChanged;

    public TPadCore Core { get; set; }
    public ITPadAppContainer Container { get; set; }
    public ITPadAppController Controller { get; set; }
    public Dictionary<String, String> Context { get { return null; } }

    public BrowserApp(TPadCore core, ITPadAppContainer container, ITPadAppController controller)
    {
      Core = core;
      Container = container;
      Controller = controller;
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

    public void LoadInitContext(Dictionary<string, string> init)
    {
      if (init == null)
        return;
      if (!init.Keys.Contains("main"))
        return;

      String url = init["main"];
      Uri myUri;
      if (Uri.TryCreate(url, UriKind.Absolute, out myUri))
        webControl1.Source = myUri;
      else
        webControl1.Source = new Uri(String.Format("https://www.google.com/search?q={0}", url));
    }

    private void btnClose_Click(object sender, RoutedEventArgs e)
    {
      Close();
    }

  }
}
