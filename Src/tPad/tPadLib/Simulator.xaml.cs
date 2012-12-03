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
using System.Windows.Shapes;
using System.IO;
using System.ComponentModel;

namespace UofM.HCI.tPab
{
  /// <summary>
  /// Interaction logic for Simulatorç.xaml
  /// </summary>
  public partial class Simulator : Window, INotifyPropertyChanged
  {

    private float widthFactor, heightFactor;
    private TPadProfile Profile { get; set; }
    private BitmapFrame DocumentBM { get; set; }

    private float WidthFactor 
    {
      get { return widthFactor; }
      set 
      {
        widthFactor = value;
        OnPropertyChanged("WidthFactor");
      }
    }

    private float HeightFactor 
    {
      get { return heightFactor; }
      set
      {
        heightFactor = value;
        OnPropertyChanged("HeightFactor");
      }
    }

    public Simulator(Application launcher, TPadProfile profile, String document)
    {
      if (!File.Exists(document))
        throw new ArgumentException(String.Format("Document \"{1}\" not found!", document));
      
      Profile = profile;
      InitializeComponent();
      DocumentBM = BitmapFrame.Create(new Uri(document, UriKind.Relative));
    }

    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged(String name)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(name));
    }

    private void CalculateFactors()
    {
      // This is the number of pixels per centimeter on the height.
      HeightFactor = (float)(iDocument.ActualHeight / Profile.DocumentSize.Height);
      // This is the number of pixels per centimeter on the width
      WidthFactor = (float)(iDocument.ActualWidth / Profile.DocumentSize.Width);

      // These two values should be nearly the same
      if (Math.Abs(HeightFactor - WidthFactor) >= 0.5)
        throw new ArgumentException("The document image does not match the specified document profile");
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      iDocument.Source = DocumentBM;
    }
  }
}
