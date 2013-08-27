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
using System.Drawing;
using System.Windows.Threading;
using UofM.HCI.tPad.Controls;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace UofM.HCI.tPad.App.InfCapture
{
  /// <summary>
  /// Interaction logic for UserControl1.xaml
  /// </summary>
  public partial class InfCaptureApp : UserControl, ITPadApp, INotifyPropertyChanged
  {

    private static log4net.ILog logger = log4net.LogManager.GetLogger(typeof(InfCaptureApp));

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

    public Exp2Condition CurrentCondition
    {
      get
      {
        if (currentConditionIndex == -1 || currentConditionIndex >= experimentalOrder.Length)
          return null;
        return conditions[experimentalOrder[currentConditionIndex]];
      }
    }

    public Exp2Capture CurrentCapture
    {
      get
      {
        if (CurrentCondition == null)
          return null;
        return CurrentCondition.Captures[currentTrial * MAX_CAPTURES_PER_TRIAL + currentCapture];
      }
    }

    private NotificationDialog notification = null;

    private Object captureLock = new Object();
    private Bitmap capture = null;

    private const int MAX_CAPTURES_PER_TRIAL = 1;
    private const int MAX_TRIALS_PER_CONDITION = 1;

    private int currentCapture = 0;
    private int currentTrial = 0;

    private int currentConditionIndex = -1;
    private int[] experimentalOrder = { 6, 7, 8, 3, 4, 5, 0, 1, 2, 9, 10, 11 };
    private Exp2Condition[] conditions = 
    {
      new Exp2Condition() { Device = Device.Normal, PictureMode = PictureMode.Normal, TargetSize = TargetSize.Quarter },
      new Exp2Condition() { Device = Device.Normal, PictureMode = PictureMode.Normal, TargetSize = TargetSize.Half },
      new Exp2Condition() { Device = Device.Normal, PictureMode = PictureMode.Normal, TargetSize = TargetSize.ThreeQuarters },
      new Exp2Condition() { Device = Device.Normal, PictureMode = PictureMode.Clipped, TargetSize = TargetSize.Quarter },
      new Exp2Condition() { Device = Device.Normal, PictureMode = PictureMode.Clipped, TargetSize = TargetSize.Half },
      new Exp2Condition() { Device = Device.Normal, PictureMode = PictureMode.Clipped, TargetSize = TargetSize.ThreeQuarters },
      new Exp2Condition() { Device = Device.tPad, PictureMode = PictureMode.Normal, TargetSize = TargetSize.Quarter },
      new Exp2Condition() { Device = Device.tPad, PictureMode = PictureMode.Normal, TargetSize = TargetSize.Half },
      new Exp2Condition() { Device = Device.tPad, PictureMode = PictureMode.Normal, TargetSize = TargetSize.ThreeQuarters },
      new Exp2Condition() { Device = Device.tPad, PictureMode = PictureMode.Clipped, TargetSize = TargetSize.Quarter },
      new Exp2Condition() { Device = Device.tPad, PictureMode = PictureMode.Clipped, TargetSize = TargetSize.Half },
      new Exp2Condition() { Device = Device.tPad, PictureMode = PictureMode.Clipped, TargetSize = TargetSize.ThreeQuarters }
    };

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

    public InfCaptureApp(TPadCore core, ITPadAppContainer container, ITPadAppController controller, Guid appUUID)
    {
      Core = core;
      Container = container;
      AppUUID = appUUID;

      InitializeComponent();

      notification = new NotificationDialog(Core, Guid.NewGuid());
      notification.ClickedOK += new EventHandler(notification_ClickedOK);
      notification.ClickedCancel += new EventHandler(notification_ClickedCancel);

      Core.Registration.OnNotifyContextServiceListeners += Registration_OnNotifyContextServiceListeners;
    }

    private IntPtr hBitmap;
    void Registration_OnNotifyContextServiceListeners(object sender, Ubicomp.Utils.NET.CAF.ContextService.NotifyContextServiceListenersEventArgs e)
    {
      Dispatcher.Invoke(DispatcherPriority.Render,
        (Action)delegate()
        {
          if (CurrentCondition == null)
            return;
          if (CurrentCondition.Device == Device.Normal && CurrentCondition.PictureMode == PictureMode.Clipped && CState == ClippingState.Clipping)
            return;

          lock (captureLock)
          {
            UofM.HCI.tPad.Services.RegistrationService registration = sender as UofM.HCI.tPad.Services.RegistrationService;

            capture = (Bitmap)registration.Tracker.GetCameraImg(CurrentCondition.Device == Device.tPad).Clone();
            if (CurrentCondition.Device == Device.Normal)
              capture.RotateFlip(RotateFlipType.Rotate90FlipXY);

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
          }
        });
    }

    public void DeActivate() { }

    public void Close()
    {
      Core.Registration.OnNotifyContextServiceListeners -= Registration_OnNotifyContextServiceListeners;

      if (Closed != null)
        Closed(this, EventArgs.Empty);
    }

    public void OnPropertyChanged(String name)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(name));
    }

    public void LoadInitContext(Dictionary<string, Object> context) { }

    private void icApp_Loaded(object sender, RoutedEventArgs e)
    {
      StartExperiment();
    }

    private void StartExperiment()
    {
      Random generator = new Random((int)DateTime.Now.Ticks);
      for (int condition = 0; condition < conditions.Length; condition++)
      {
        for (int capture = 0; capture < MAX_TRIALS_PER_CONDITION * MAX_CAPTURES_PER_TRIAL; capture++)
        {
          Exp2Capture task = new Exp2Capture();
          //Page depends on the target size - each page has only 1 target size, 3 pages have the same target size
          if (capture % MAX_CAPTURES_PER_TRIAL == 0) //new page
          {
            do
            {
              task.Page = (int)conditions[condition].TargetSize * 3 + (generator.Next() % 3);
            } while (conditions[condition].Captures.Exists(existingTask => existingTask.Page == task.Page));
          }
          else
            task.Page = conditions[condition].Captures[capture - 1].Page;
          task.Figure = capture % MAX_CAPTURES_PER_TRIAL + 1;
          conditions[condition].Captures.Add(task);
        }
      }

      currentConditionIndex++;
      OnPropertyChanged("CurrentCondition");
      OnPropertyChanged("CurrentCapture");

      String message = String.Format("Next Capture\n\rDevice: {0}\nTechnique: {1}\n\rPage: {2} - Figure {3}", CurrentCondition.Device, CurrentCondition.PictureMode, CurrentCapture.Page, CurrentCapture.Figure);
      MessageBoxShow(message, "READY", "CANCEL");
    }

    private void NextTask()
    {
      currentCapture = (currentCapture + 1) % MAX_CAPTURES_PER_TRIAL;
      if (currentCapture == 0) //goes on to next trial
      {
        currentTrial = (currentTrial + 1) % MAX_TRIALS_PER_CONDITION;
        if (currentTrial == 0) //finished the condition goes on to the next
          currentConditionIndex++;
      }

      CState = ClippingState.Capturing;
      OnPropertyChanged("CurrentCondition");
      OnPropertyChanged("CurrentCapture");
      TranslateX = 0;
      TranslateY = 0;
      Angle = 0;
      SideSize = 200;

      if (currentConditionIndex < conditions.Length)
      {
        String message = String.Format("Next Capture\n\rDevice: {0}\nTechnique: {1}\n\rPage: {2} - Figure {3}", CurrentCondition.Device, CurrentCondition.PictureMode, CurrentCapture.Page, CurrentCapture.Figure);
        MessageBoxShow(message, "READY", "CANCEL");
      }
      else
      {
        MessageBoxShow("Experiment Finished", "OK", "CANCEL");
      }
    }

    private void MessageBoxShow(String message, String yesButton, String noButton)
    {
      Dictionary<String, Object> context = new Dictionary<String, Object>();
      context.Add("message", message);
      context.Add("buttonOK", yesButton);
      context.Add("buttonCancel", noButton);
      context.Add("sender", null);
      context.Add("currentApp", null);

      notification.LoadInitContext(context);
      Container.LoadTPadApp(notification, true);
    }

    void notification_ClickedOK(object sender, EventArgs e)
    {
      if (CurrentCapture == null)
        return;

      CurrentCapture.StartTime = DateTime.Now;
    }

    void notification_ClickedCancel(object sender, EventArgs e)
    {
    }

    private ClippingState cState = ClippingState.Capturing;
    public ClippingState CState
    {
      get { return cState; }
      set
      {
        cState = value;
        OnPropertyChanged("CState");
      }
    }

    private void bCapture_Click(object sender, RoutedEventArgs e)
    {
      if (CurrentCondition == null)
        return;

      if (CurrentCondition.Device == Device.Normal)
      {
        if (CurrentCondition.PictureMode == PictureMode.Normal)
        {
          CurrentCapture.EndTime = DateTime.Now;
          SaveImageAndLog(true);
          NextTask();
        }
        else if (CurrentCondition.PictureMode == PictureMode.Clipped)
        {
          if (CState == ClippingState.Capturing)
            CState = ClippingState.Clipping;
          else if (CState == ClippingState.Clipping)
          {
            CurrentCapture.EndTime = DateTime.Now;
            SaveImageAndLog(true);
            NextTask();
          }
        }
      }

      else if (CurrentCondition.Device == Device.tPad)
      {
        CurrentCapture.EndTime = DateTime.Now;
        SaveImageAndLog(true);
        NextTask();
      }
    }

    private void SaveImageAndLog(bool drawMarkers)
    {
      //saves the image with the target marks
      long ticks = CurrentCapture.EndTime.Ticks;
      String fileName = String.Format("{0}/{1}-{2}-{3}-{4}-trial{5}-capture{6}.jpg", Environment.CurrentDirectory, ticks, CurrentCondition.Device, CurrentCondition.PictureMode, CurrentCondition.TargetSize, currentTrial, currentCapture);
      lock (captureLock)
      {
        //Writes the current PictureMode
        RectangleF rectf = new RectangleF(5, capture.Height - 20, 100, 20);
        Graphics g = Graphics.FromImage(capture);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        g.DrawString(CurrentCondition.PictureMode.ToString(), new Font("Thaoma", 8), System.Drawing.Brushes.Red, rectf);
        g.Flush();

        //Draw the target marks on the figure
        if (drawMarkers)
        {
          System.Drawing.Drawing2D.Matrix transform = new System.Drawing.Drawing2D.Matrix();
          transform.RotateAt(Angle, new PointF(capture.Width / 2 + TranslateX, capture.Height / 2 + TranslateY));
          transform.Translate(TranslateX, TranslateY);
          g.Transform = transform;

          System.Drawing.Rectangle rect = new System.Drawing.Rectangle();
          rect.Width = SideSize;
          rect.Height = SideSize;
          rect.X = (int)(capture.Width - SideSize) / 2;
          rect.Y = (int)(capture.Height - SideSize) / 2;
          g.DrawRectangle(Pens.Red, rect);

          g.DrawLine(Pens.Red,
            new PointF(capture.Width / 2, capture.Height / 2 - 100),
            new PointF(capture.Width / 2, capture.Height / 2 + 100));
          g.DrawLine(Pens.Red,
            new PointF(capture.Width / 2 - 100, capture.Height / 2),
            new PointF(capture.Width / 2 + 100, capture.Height / 2));
          g.Flush();
        }

        //Save the figure
        capture.Save(fileName, System.Drawing.Imaging.ImageFormat.Png);
      }

      //logs the selection as finished
      String logLine = String.Format("{0};{1};{2};{3};{4};{5};{6}",
        ticks,
        CurrentCondition.Device,
        CurrentCondition.PictureMode,
        CurrentCondition.TargetSize,
        currentTrial,
        currentCapture,
        CurrentCapture.CaptureTime.TotalMilliseconds);
      logger.Info(logLine);
    }

    private void bRetake_Click(object sender, RoutedEventArgs e)
    {
      CState = ClippingState.Capturing;
    }

    private bool isMoving = false;
    private System.Windows.Point startingPoint = new System.Windows.Point(0, 0);
    private void eTranslate_MouseDown(object sender, MouseButtonEventArgs e)
    {
      if (CurrentCondition == null)
        return;
      if (CurrentCondition.Device == Device.Normal && CState == ClippingState.Capturing)
        return;
      if (CurrentCondition.Device == Device.tPad && CurrentCondition.PictureMode == PictureMode.Normal)
        return;

      isMoving = true;
      startingPoint = e.GetPosition(this);
      startingPoint.X -= TranslateX;
      startingPoint.Y -= TranslateY;
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
    }

    private void eTranslate_MouseLeave(object sender, MouseEventArgs e)
    {
      //isMoving = false;
    }

    private void eTranslate_MouseEnter(object sender, MouseEventArgs e)
    {
      //isMoving = false;
    }

    private bool isRotatingScaling = false;
    private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
    {
      if (CurrentCondition == null)
        return;
      if (CurrentCondition.Device == Device.Normal && CState == ClippingState.Capturing)
        return;
      if (CurrentCondition.Device == Device.tPad && CurrentCondition.PictureMode == PictureMode.Normal)
        return;

      isRotatingScaling = true;
      ResizeMarkers(sender, e);
    }

    private void Rectangle_MouseMove(object sender, MouseEventArgs e)
    {
      if (!isRotatingScaling)
        return;

      ResizeMarkers(sender, e);
    }

    private void ResizeMarkers(object sender, MouseEventArgs e)
    {
      var rect = sender as System.Windows.Shapes.Rectangle;

      System.Windows.Point point = e.GetPosition(rect);
      var axis = point - new System.Windows.Point(rect.ActualWidth / 2 + TranslateX, rect.ActualHeight / 2 + TranslateY);

      SideSize = (int)axis.Length * 2;
      if (axis.Y == 0)
        Angle = 90;
      else
        Angle = -1 * (int)(Math.Atan(axis.X / axis.Y) * 180 / Math.PI);
    }

    private void Rectangle_MouseUp(object sender, MouseButtonEventArgs e)
    {
      isRotatingScaling = false;
    }

    private void Rectangle_MouseLeave(object sender, MouseEventArgs e)
    {
      //isRotatingScaling = false;
    }

    [DllImport("gdi32.dll")]
    private static extern bool DeleteObject(IntPtr hObject);

  }

}
