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
    public event RequestAction RequestAction;

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
        if (currentConditionIndex == -1 || currentConditionIndex >= ExperimentalOrder.Length)
          return null;

        while (ExperimentalOrder[currentConditionIndex] - 1 >= Conditions.Count)
        {
          currentConditionIndex++;
          if (currentConditionIndex >= ExperimentalOrder.Length)
            return null;
        }

        return Conditions[ExperimentalOrder[currentConditionIndex] - 1];
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

    private const int MAX_CAPTURES_PER_TRIAL = 3;
    private const int MAX_TRIALS_PER_CONDITION = 3;

    private int currentCapture = 0;
    private int currentTrial = 0;

    private int currentConditionIndex = -1;

    public int[] ExperimentalOrder { get; set; }
    public List<Exp2Condition> Conditions { get; set; }

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
      UofM.HCI.tPad.Services.RegistrationService registration = sender as UofM.HCI.tPad.Services.RegistrationService;
      if (registration.Tracker == null)
        return;

      if (CurrentCondition != null && CurrentCondition.Device == Device.Normal && CurrentCondition.PictureMode == PictureMode.Clipped && CState == ClippingState.Clipping)
        return;

      Dispatcher.Invoke(DispatcherPriority.Render,
        (Action)delegate()
        {
          lock (captureLock)
          {
            bool warped = false;
            if (CurrentCondition != null && CurrentCondition.Device == Device.tPad)
              warped = true;
            capture = (Bitmap)registration.Tracker.GetCameraImg(warped).Clone();
            if (CurrentCondition == null || CurrentCondition.Device == Device.Normal)
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

    public void Activate(Dictionary<string, Object> context) { }

    private void bStart_Click(object sender, RoutedEventArgs e)
    {
      bStart.Visibility = System.Windows.Visibility.Collapsed;
      bCameraFeed.IsChecked = false;
      StartExperiment();
    }

    private void StartExperiment()
    {
      Random generator = new Random((int)DateTime.Now.Ticks);
      for (int condition = 0; condition < Conditions.Count; condition++)
      {
        for (int capture = 0; capture < MAX_TRIALS_PER_CONDITION * MAX_CAPTURES_PER_TRIAL; capture++)
        {
          Exp2Capture task = new Exp2Capture();
          //Page depends on the target size - each page has only 1 target size, 3 pages have the same target size
          if (capture % MAX_CAPTURES_PER_TRIAL == 0) //new page
          {
              task.Page = (int)Conditions[condition].TargetSize;
          }
          else
            task.Page = Conditions[condition].Captures[capture - 1].Page;
          task.Figure = generator.Next(MAX_CAPTURES_PER_TRIAL) + 1;
          Conditions[condition].Captures.Add(task);
        }
      }

      currentConditionIndex = 0; ;
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

      if (CurrentCondition != null && CurrentCondition.Device == Device.tPad && CurrentCondition.PictureMode == PictureMode.Clipped)
        CState = ClippingState.Clipping;

      if (CurrentCondition != null)
      {
        String message = String.Format("Next Capture\n\rDevice: {0}\nTechnique: {1}\n\rPage: {2} - Figure {3}", CurrentCondition.Device, CurrentCondition.PictureMode, CurrentCapture.Page, CurrentCapture.Figure);
        MessageBoxShow(message, "READY", "CANCEL");
      }
      else
      {
        MessageBoxShow("Experiment Finished", "OK", "CANCEL");
        bStart.Visibility = System.Windows.Visibility.Visible;
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

      notification.Activate(context);
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
          SaveImageAndLog(true, false);
          NextTask();
        }
        else if (CurrentCondition.PictureMode == PictureMode.Clipped)
        {
          if (CState == ClippingState.Capturing)
            CState = ClippingState.Clipping;
          else if (CState == ClippingState.Clipping)
          {
            CurrentCapture.EndTime = DateTime.Now;
            SaveImageAndLog(true, true);
            NextTask();
          }
        }
      }

      else if (CurrentCondition.Device == Device.tPad)
      {
        CurrentCapture.EndTime = DateTime.Now;
        SaveImageAndLog(false, false);
        NextTask();
      }
    }

    private void SaveImageAndLog(bool drawTarget, bool drawBorders)
    {
      //saves the image with the target marks
      long ticks = CurrentCapture.EndTime.Ticks;
      String fileName = String.Format("{0}/{1}-{2}-{3}-{4}-trial{5}-capture{6}.jpg", Environment.CurrentDirectory, ticks, CurrentCondition.Device, CurrentCondition.PictureMode, CurrentCondition.TargetSize, currentTrial, currentCapture);
      lock (captureLock)
      {
        //Writes the current PictureMode
        RectangleF rectf = new RectangleF(5, capture.Height - 40, 100, 30);
        Graphics g = Graphics.FromImage(capture);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        g.DrawString(String.Format("Device: {0} Mode: {1}", CurrentCondition.Device, CurrentCondition.PictureMode), new Font("Thaoma", 8), System.Drawing.Brushes.Red, rectf);
        g.Flush();

        float ratio = 1;
        if (drawBorders || drawTarget)
          ratio = (float)(iDeviceCameraFeed.ActualHeight / capture.Height);

        System.Drawing.Drawing2D.Matrix transform = new System.Drawing.Drawing2D.Matrix();
        transform.RotateAt(Angle, new PointF(capture.Width / 2 + TranslateX, capture.Height / 2 + TranslateY));
        transform.Scale((float)(1/ratio), (float)(1/ratio));
        transform.Translate(TranslateX, TranslateY);
        g.Transform = transform;

        //Draw the target marks on the figure
        if (drawBorders)
        {
          System.Drawing.Rectangle rect = new System.Drawing.Rectangle();
          rect.Width = SideSize;
          rect.Height = SideSize;
          rect.X = (int)(iDeviceCameraFeed.ActualWidth - SideSize) / 2;
          rect.Y = (int)(iDeviceCameraFeed.ActualHeight - SideSize) / 2;
          g.DrawRectangle(Pens.Red, rect);
        }

        if (drawTarget)
        {
          g.DrawLine(Pens.Red,
            new PointF((float)(iDeviceCameraFeed.ActualWidth / 2), (float)(iDeviceCameraFeed.ActualHeight / 2 - 50)),
            new PointF((float)(iDeviceCameraFeed.ActualWidth / 2), (float)(iDeviceCameraFeed.ActualHeight / 2 + 50)));
          g.DrawLine(Pens.Red,
            new PointF((float)(iDeviceCameraFeed.ActualWidth / 2 - 50), (float)(iDeviceCameraFeed.ActualHeight / 2)),
            new PointF((float)(iDeviceCameraFeed.ActualWidth / 2 + 50), (float)(iDeviceCameraFeed.ActualHeight / 2)));
        }

        g.Flush();

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

    private void Rectangle_MouseUp(object sender, MouseButtonEventArgs e)
    {
      isMoving = false;
      isRotatingScaling = false;
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

    private void Rectangle_MouseLeave(object sender, MouseEventArgs e)
    {
      //isRotatingScaling = false;
    }

    [DllImport("gdi32.dll")]
    private static extern bool DeleteObject(IntPtr hObject);

  }

}