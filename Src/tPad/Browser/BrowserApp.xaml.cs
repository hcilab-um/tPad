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

    public event BoolEventHandler IsTopApp;
    public event RequestUserFocus RequestFocus;
    public event EventHandler Closed;
    public event PropertyChangedEventHandler PropertyChanged;

    public Guid AppUUID { get; private set; }
    public TPadCore Core { get; set; }
    public ITPadAppContainer Container { get; set; }
    public ITPadAppController Controller { get; set; }

    public BrowserApp(TPadCore core, ITPadAppContainer container, ITPadAppController controller, Guid appUUID)
    {
      AppUUID = appUUID;
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

    public void LoadInitContext(Dictionary<string, Object> init)
    {
      if (init == null)
        return;
      if (!init.Keys.Contains("main"))
        return;

      String url = init["main"] as String;
      Uri myUri;
      if (Uri.TryCreate(url, UriKind.Absolute, out myUri))
        webControl1.Source = myUri;
      else
        webControl1.Source = new Uri(String.Format("https://www.google.com/search?q={0}", url));
    }

    private bool tapAndFlip = false;
    private void webControl1_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
      tapAndFlip = true;
      Point position = e.GetPosition(webControl1);
      webControl1.CopyImageAt((int)position.X, (int)position.Y);
    }

    private void webControl1_PreviewMouseUp(object sender, MouseButtonEventArgs e)
    {
      tapAndFlip = false;
      Clipboard.Clear();
    }

    public Dictionary<string, Object> Context
    {
      get
      {
        if (!tapAndFlip)
          return null;

        Console.WriteLine(Clipboard.ContainsImage());
        if (Clipboard.ContainsImage())
        {
          BitmapSource image = Clipboard.GetImage();
          Clipboard.Clear();

          Dictionary<string, object> context = new Dictionary<string, object>();
          context.Add("image", image);
          return context;
        }
        return null;
      }
    }

  }
}
