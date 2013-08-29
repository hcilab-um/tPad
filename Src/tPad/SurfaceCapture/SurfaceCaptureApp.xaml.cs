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
using System.Windows.Threading;
using System.Drawing;
using System.Runtime.InteropServices;

namespace UofM.HCI.tPad.App.SurfaceCapture
{
  /// <summary>
  /// Interaction logic for UserControl1.xaml
  /// </summary>
  public partial class SurfaceCaptureApp : UserControl, ITPadApp, INotifyPropertyChanged
  {

    public event BoolEventHandler IsTopApp;
    public event RequestUserFocus RequestFocus;
    public event EventHandler Closed;
    public event PropertyChangedEventHandler PropertyChanged;

    public Guid AppUUID { get; private set; }
    public TPadCore Core { get; set; }
    public ITPadAppContainer Container { get; set; }
    public ITPadAppController Controller { get; set; }

    public Dictionary<String, Object> Context
    {
      get { return null; }
    }

    private int translateX = 0;
    public int TranslateX
    {
      get { return translateX; }
      set
      {
        translateX = value;
        OnPropertyChanged("TranslateX");
      }
    }

    private int translateY = 0;
    public int TranslateY
    {
      get { return translateY; }
      set
      {
        translateY = value;
        OnPropertyChanged("TranslateY");
      }
    }

    private int sideSize = 200;
    public int SideSize
    {
      get { return sideSize; }
      set
      {
        sideSize = value;
        OnPropertyChanged("SideSize");
      }
    }

    private int angle = 0;
    public int Angle
    {
      get { return angle; }
      set
      {
        angle = value;
        OnPropertyChanged("Angle");
      }
    }

    public SurfaceCaptureApp(TPadCore core, ITPadAppContainer container, ITPadAppController controller, Guid appUUID)
    {
      Core = core;
      Container = container;
      AppUUID = appUUID;

      InitializeComponent();
    }

    public void Activate(Dictionary<string, Object> context)
    {
      Core.Registration.OnNotifyContextServiceListeners += Registration_OnNotifyContextServiceListeners;
    }

    public void DeActivate()
    {
      Core.Registration.OnNotifyContextServiceListeners -= Registration_OnNotifyContextServiceListeners;
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

    [DllImport("gdi32.dll")]
    private static extern bool DeleteObject(IntPtr hObject);

    private IntPtr hBitmap = IntPtr.Zero;
    void Registration_OnNotifyContextServiceListeners(object sender, Ubicomp.Utils.NET.CAF.ContextService.NotifyContextServiceListenersEventArgs e)
    {
      Dispatcher.Invoke(DispatcherPriority.Render,
        (Action)delegate()
        {
          UofM.HCI.tPad.Services.RegistrationService registration = sender as UofM.HCI.tPad.Services.RegistrationService;
          Bitmap capture = (Bitmap)registration.Tracker.GetCameraImg(true).Clone();

          IntPtr tmpPointer = capture.GetHbitmap();
          iDeviceCameraFeed.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
            tmpPointer,
            IntPtr.Zero,
            Int32Rect.Empty,
            System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

          if (hBitmap != IntPtr.Zero)
            DeleteObject(hBitmap);
          hBitmap = tmpPointer;
          GC.Collect(0, GCCollectionMode.Forced);
        });
    }

    private void bCapture_Click(object sender, RoutedEventArgs e)
    {
      Bitmap capture = (Bitmap)Core.Registration.Tracker.GetCameraImg(true).Clone();

      if (bClipping.IsChecked == true)
      {
        float ratio = (float)(ActualHeight / capture.Height);
        float multiplier = 1 / ratio;

        //Transformation for the cropping rectangle
        //System.Drawing.Drawing2D.Matrix transform = new System.Drawing.Drawing2D.Matrix();
        //transform.RotateAt(Angle, new PointF(capture.Width / 2 + TranslateX, capture.Height / 2 + TranslateY));
        //transform.Scale((float)(1 / ratio), (float)(1 / ratio));
        //transform.Translate(TranslateX, TranslateY);

        //Cropping rectangle
        System.Drawing.RectangleF rect = new System.Drawing.RectangleF();
        rect.Width = SideSize;
        rect.Height = SideSize;
        rect.X = (int)(ActualWidth - SideSize) / 2 + TranslateX;
        rect.Y = (int)(ActualHeight - SideSize) / 2 + TranslateY;

        rect.Width *= multiplier;
        rect.Height *= multiplier;
        rect.X *= multiplier;
        rect.Y *= multiplier;

        capture = CropCapture(capture, rect);
      }

      IntPtr tmpPointer = capture.GetHbitmap();
      iPreview.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
        tmpPointer,
        IntPtr.Zero,
        Int32Rect.Empty,
        System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
      iPreview.Width = capture.Width;
      iPreview.Height = capture.Height;
      gPreview.Visibility = System.Windows.Visibility.Visible;
      bClipping.IsChecked = false;

      String path = String.Format("{0}\\Device{1}\\Photos\\{2}.jpg", Environment.CurrentDirectory, Core.Device.ID, DateTime.Now.Ticks);
      capture.Save(path, System.Drawing.Imaging.ImageFormat.Jpeg);

      //DeleteObject(tmpPointer);
      GC.Collect(0, GCCollectionMode.Forced);
    }

    private void iDeviceCameraFeed_MouseUp(object sender, MouseButtonEventArgs e)
    {
      gPreview.Visibility = System.Windows.Visibility.Collapsed;
    }

    private Bitmap CropCapture(Bitmap capture, System.Drawing.RectangleF croppingArea)
    {
      Bitmap target = new Bitmap((int)croppingArea.Width, (int)croppingArea.Height);
      using (Graphics g = Graphics.FromImage(target))
      {
        g.DrawImage(capture, new System.Drawing.Rectangle(0, 0, target.Width, target.Height), croppingArea, GraphicsUnit.Pixel);
      }

      return target;
    }


    private bool isMoving = false;
    private System.Windows.Point startingPoint = new System.Windows.Point(0, 0);
    private void eTranslate_MouseDown(object sender, MouseButtonEventArgs e)
    {
      isMoving = true;
      startingPoint = e.GetPosition(this);
      startingPoint.X -= TranslateX;
      startingPoint.Y -= TranslateY;
      e.Handled = true;
    }

    private void eTranslate_MouseMove(object sender, MouseEventArgs e)
    {
      if (!isMoving)
        return;

      System.Windows.Point point = e.GetPosition(this);
      var diff = point - startingPoint;
      TranslateX = (int)diff.X;
      TranslateY = (int)diff.Y;
    }

    private void eTranslate_MouseUp(object sender, MouseButtonEventArgs e)
    {
      isMoving = false;
      isRotatingScaling = false;
    }

    private void eTranslate_MouseLeave(object sender, MouseEventArgs e)
    {

    }

    private void eTranslate_MouseEnter(object sender, MouseEventArgs e)
    {

    }

    private bool isRotatingScaling = false;
    private void scApp_MouseDown(object sender, MouseButtonEventArgs e)
    {
      if (bClipping.IsChecked == false)
        return;

      isRotatingScaling = true;
      ResizeMarkers(sender, e);
      e.Handled = true;
    }

    private void scApp_MouseMove(object sender, MouseEventArgs e)
    {
      if (!isRotatingScaling)
        return;

      ResizeMarkers(sender, e);
    }

    private void scApp_MouseUp(object sender, MouseButtonEventArgs e)
    {
      isMoving = false;
      isRotatingScaling = false;
    }

    private void rClippingArea_MouseUp(object sender, MouseButtonEventArgs e)
    {
      isMoving = false;
      isRotatingScaling = false;
    }

    private void ResizeMarkers(object sender, MouseEventArgs e)
    {
      System.Windows.Point point = e.GetPosition(this);
      var axis = point - new System.Windows.Point(ActualWidth / 2 + TranslateX, ActualHeight / 2 + TranslateY);

      SideSize = (int)axis.Length * 2;
      //if (axis.Y == 0)
      //  Angle = 90;
      //else
      //  Angle = -1 * (int)(Math.Atan(axis.X / axis.Y) * 180 / Math.PI);
    }

    private void bClipping_Click(object sender, RoutedEventArgs e)
    {
      if (bClipping.IsChecked == true)
        bClipping.Content = "Normal";
      else
        bClipping.Content = "Clip";
    }

  }

}
