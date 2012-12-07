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
using UofM.HCI.tPab.Util;

namespace UofM.HCI.tPab
{
  /// <summary>
  /// Interaction logic for Simulatorç.xaml
  /// </summary>
  public partial class Simulator : Window, INotifyPropertyChanged
  {

    private float widthFactor, heightFactor;
    private float rotationAngle;
    private BitmapFrame DocumentBM { get; set; }
    private UserControl TPadApp { get; set; }

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

    public float RotationAngle
    {
      get { return rotationAngle; }
      set
      {
        rotationAngle = value;
        OnPropertyChanged("RotationAngle");
      }
    }

    public Simulator(Application launcher, String document, UserControl app = null)
    {
      if (!File.Exists(document))
        throw new ArgumentException(String.Format("Document \"{1}\" not found!", document));

      InitializeComponent();
      iDocument.SizeChanged += new SizeChangedEventHandler(iDocument_SizeChanged);
      DocumentBM = BitmapFrame.Create(new Uri(document, UriKind.Relative));

      if (app != null && app is ITPadApp)
      {
        TPadApp = app;
        TPadApp.VerticalAlignment = System.Windows.VerticalAlignment.Center;
        TPadApp.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
        gTPadApp.Children.Add(TPadApp);
      }
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      iDocument.Source = DocumentBM;
    }

    void iDocument_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      CalculateFactors();
    }

    private void CalculateFactors()
    {
      // This is the number of pixels per centimeter on the height.
      HeightFactor = (float)(iDocument.ActualHeight / TPadCore.Instance.Profile.DocumentSize.Height);
      // This is the number of pixels per centimeter on the width
      WidthFactor = (float)(iDocument.ActualWidth / TPadCore.Instance.Profile.DocumentSize.Width);

      // These two values should be nearly the same
      if (Math.Abs(HeightFactor - WidthFactor) >= 0.5)
        throw new ArgumentException("The document image does not match the specified document profile");

      //Resize the device
      gTPadApp.Width = WidthFactor * TPadCore.Instance.Profile.DeviceSize.Width;
      gTPadApp.Height = HeightFactor * TPadCore.Instance.Profile.DeviceSize.Height;
      //Adjusts the screen size to the device size
      TPadApp.Width = WidthFactor * TPadCore.Instance.Profile.ScreenSize.Width;
      TPadApp.Height = HeightFactor * TPadCore.Instance.Profile.ScreenSize.Height;
    }

    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged(String name)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(name));
    }

    private bool isTraslating = false, isRotating = false;
    private Point lastPosition;
    private void rFrame_MouseDown(object sender, MouseButtonEventArgs e)
    {
      if (e.LeftButton == MouseButtonState.Pressed && e.RightButton == MouseButtonState.Released)
      {
        isTraslating = true;
        lastPosition = Mouse.GetPosition(this);
      }
      else if (e.LeftButton == MouseButtonState.Released && e.RightButton == MouseButtonState.Pressed)
      {
        isRotating = true;
        lastPosition = Mouse.GetPosition(this);
      }
    }

    private void rFrame_MouseMove(object sender, MouseEventArgs e)
    {
      if (isTraslating)
      {
        //Gets the new position and checks whether there has been any movement since last time
        Point newPosition = Mouse.GetPosition(this);
        if (newPosition == lastPosition)
          return;

        //Finds how much the mouse moved from last frame
        Vector displacement = newPosition - lastPosition;

        //Replaces the last position
        lastPosition = newPosition;

        //Adds such displacement to the current position of the app control
        Point currentLocation = new Point(gTPadApp.Margin.Left, gTPadApp.Margin.Top);
        Point newLocation = currentLocation + displacement;
        gTPadApp.Margin = new Thickness(newLocation.X, newLocation.Y, 0, 0);
      }
      else if (isRotating)
      {
        //Gets the new position and checks whether there has been any movement since last time
        Point newPosition = Mouse.GetPosition(this);
        if (newPosition == lastPosition)
          return;

        //Finds how much the mouse moved from last frame
        Vector displacement = newPosition - lastPosition;

        //Replaces the last position
        lastPosition = newPosition;

        //Adds such displacement to the current position of the app control
        RotationAngle -= (float)displacement.X;
      }
    }

    private void rFrame_MouseUp(object sender, MouseButtonEventArgs e)
    {
      isTraslating = false;
      isRotating = false;
    }

    public System.Drawing.Bitmap GetDeviceView(out float angle)
    {
      angle = RotationAngle;
      GetDeviceViewDelegate gdvDelegate = new GetDeviceViewDelegate(SafeGetDeviceView);
      Object[] args = new Object[0];
      MemoryStream result = (MemoryStream)Dispatcher.Invoke(gdvDelegate, args);
      if (result == null)
        return null;

      System.Drawing.Bitmap frame = new System.Drawing.Bitmap(result);
      return frame;
    }

    private Rect tPadBounds = Rect.Empty;
    private MemoryStream SafeGetDeviceView()
    {
      if (!IsActive)
        return null;

      int bordersize = (int)(RestoreBounds.Width - gTop.ActualWidth) / 2;
      int bordertop = (int)(RestoreBounds.Height - gTop.ActualHeight - bordersize);
      int zeroX = (int)(Left + bordersize);
      int zeroY = (int)(Top + bordertop);

      if (tPadBounds == Rect.Empty)
        tPadBounds = VisualTreeHelper.GetDescendantBounds(gTPadApp);
      Rect ttPadBounds = gTPadApp.TransformToAncestor(this).TransformBounds(tPadBounds);

      System.Drawing.Bitmap capture = ImageHelper.ScreenCapture(zeroX + ttPadBounds.Left, zeroY + ttPadBounds.Top, ttPadBounds.Width, ttPadBounds.Height);

      //Point relativePoint = gTop.TransformToAncestor(this).Transform(new Point(0, 0));
      //BitmapSource deviceView = ImageHelper.Crop(fullBitmap, relativePoint, tPadBounds, RotationAngle);
      MemoryStream result = new MemoryStream();
      try
      {
        capture.Save(result, System.Drawing.Imaging.ImageFormat.Png);
      }
      catch (Exception ex)
      { }

      return result;
    }

    private delegate MemoryStream GetDeviceViewDelegate();

    #region Image Cropping

    /// <summary>
    /// http://dog-net.org/content/development/wpf/bitmapsource-to-from-bitmapimage/
    /// tested.
    /// </summary>
    /// <param name="bitmapSource"></param>
    /// <returns></returns>
    public static BitmapImage bitmapOfBitmapSource(BitmapSource bitmapSource)
    {
      JpegBitmapEncoder encoder = new JpegBitmapEncoder();
      MemoryStream memoryStream = new MemoryStream();
      BitmapImage bImg = new System.Windows.Media.Imaging.BitmapImage();

      encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
      encoder.Save(memoryStream);

      bImg.BeginInit();
      bImg.StreamSource = new MemoryStream(memoryStream.ToArray());
      bImg.EndInit();

      memoryStream.Close();
      return bImg;
    }

    public static byte[] ByteArrayOfBitmapImage(BitmapImage imageC)
    {
      MemoryStream memStream = new MemoryStream();
      JpegBitmapEncoder encoder = new JpegBitmapEncoder();
      encoder.Frames.Add(BitmapFrame.Create(imageC));
      encoder.Save(memStream);
      return memStream.GetBuffer();
    }

    private static BitmapImage BitmapImageOfByteArray(byte[] rawImageBytes)
    {
      BitmapImage imageSource = null;
      var stream = new MemoryStream(rawImageBytes);
      {
        stream.Seek(0, SeekOrigin.Begin);
        BitmapImage b = new BitmapImage();
        b.BeginInit();
        b.StreamSource = stream;
        b.EndInit();
        b.Freeze();
        imageSource = b;
      }
      return imageSource;
    }

    //[OperationContract]
    //public void CropImageOnServer(string imageDir, string imageFileName, TipoImmagine tipo, int x0, int y0, int x1, int y1)
    //{
    //  var imagePath = Path.Combine(imageDir, imageFileName);
    //  var by = Download(imagePath);

    //  var bi = BitmapImageOfByteArray(by);

    //  var wt = x1 - x0;
    //  var ht = y1 - y0;
    //  var crb = new CroppedBitmap(bi, new System.Windows.Int32Rect(x0, y0, wt, ht));

    //  var by2 = ByteArrayOfBitmapImage(bitmapOfBitmapSource(crb));
    //  var fname = Path.GetFileNameWithoutExtension(imagePath);
    //  var ext = Path.GetExtension(imagePath);
    //  var newFilePath = Path.Combine(imageDir, fname + "_cropped" + ext);
    //  var res = Upload(newFilePath, by2);
    //}

    #endregion

  }

}
