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
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Threading;
using ZXing;

namespace UofM.HCI.tPad.App.QReader
{
  /// <summary>
  /// Interaction logic for QReaderApp.xaml
  /// </summary>
  public partial class QReaderApp : UserControl, ITPadApp, INotifyPropertyChanged
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

    private bool foundCode = false;
    public bool FoundCode
    {
      get { return foundCode; }
      set
      {
        foundCode = value;
        OnPropertyChanged("FoundCode");
      }
    }

    private String codeType = String.Empty;
    public String CodeType
    {
      get { return codeType; }
      set
      {
        codeType = value;
        OnPropertyChanged("CodeType");
      }
    }

    private String codeContent = String.Empty;
    public String CodeContent
    {
      get { return codeContent; }
      set
      {
        codeContent = value;
        OnPropertyChanged("CodeContent");
      }
    }

    private Object captureLock = new Object();
    private Bitmap capture = null;

    public QReaderApp(TPadCore core, ITPadAppContainer container, ITPadAppController controller, Guid appUUID)
    {
      AppUUID = appUUID;
      Core = core;
      Container = container;
      Controller = controller;

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

    void Registration_OnNotifyContextServiceListeners(object sender, Ubicomp.Utils.NET.CAF.ContextService.NotifyContextServiceListenersEventArgs e)
    {
      Dispatcher.Invoke(DispatcherPriority.Render,
        (Action)delegate()
        {
          lock (captureLock)
          {
            UofM.HCI.tPad.Services.RegistrationService registration = sender as UofM.HCI.tPad.Services.RegistrationService;
            capture = (Bitmap)registration.Tracker.GetCameraImg(true).Clone();

            // create a barcode reader instance
            IBarcodeReader reader = new BarcodeReader();

            // detect and decode the barcode inside the bitmap
            var result = reader.Decode(capture);
            // do something with the result
            if (result != null)
            {
              FoundCode = true;
              CodeType = result.BarcodeFormat.ToString();
              CodeContent = result.Text;

              float ratioX = (float)ActualWidth / capture.Width;
              float ratioY = (float)ActualHeight / capture.Height;
              TranslateX = (int)(result.ResultPoints.Average(point => point.X) * ratioX) - (int)(ActualWidth / 2);
              TranslateY = (int)(result.ResultPoints.Average(point => point.Y) * ratioY) - (int)(ActualHeight / 2);
            }

            GC.Collect(0, GCCollectionMode.Forced);
          }
        });
    }

    private void bLaunch_Click(object sender, RoutedEventArgs e)
    {
      if (!FoundCode)
        return;
    }
  }
}
