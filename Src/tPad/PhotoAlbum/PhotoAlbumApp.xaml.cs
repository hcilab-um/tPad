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
using System.Collections.ObjectModel;

namespace UofM.HCI.tPad.App.PhotoAlbum
{
  /// <summary>
  /// Interaction logic for PhotoAlbumApp.xaml
  /// </summary>
  public partial class PhotoAlbumApp : UserControl, ITPadApp, INotifyPropertyChanged
  {

    public event EventHandler Closed;
    public event PropertyChangedEventHandler PropertyChanged;

    public TPadCore Core { get; set; }
    public ITPadAppContainer Container { get; set; }
    public ITPadAppController Controller { get; set; }

    public Dictionary<string, string> Context { get { return null; } }

    public ObservableCollection<String> Photos { get; set; }

    public PhotoAlbumApp(TPadCore core, ITPadAppContainer container, ITPadAppController controller)
    {
      Core = core;
      Container = container;
      Controller = controller;

      Photos = new ObservableCollection<String>();
      InitializeComponent();
      LoadPhotos();
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

    private void LoadPhotos()
    {
      Photos.Clear();
      String path = String.Format("{0}\\Device{1}\\Photos\\", Environment.CurrentDirectory, Core.Device.ID);
      var photos = System.IO.Directory.EnumerateFiles(path, "*.jpg");
      foreach (String photo in photos)
        Photos.Add(photo);
    }

    public void LoadInitContext(Dictionary<string, string> init) { }

    private void Image_MouseUp(object sender, MouseButtonEventArgs e)
    {
      imgZoom.Source = (sender as Image).Source;
      gZoom.Visibility = System.Windows.Visibility.Visible;
    }

    private void btnCloseZoom_Click(object sender, RoutedEventArgs e)
    {
      imgZoom.Source = null;
      gZoom.Visibility = System.Windows.Visibility.Collapsed;
    }

  }
}
