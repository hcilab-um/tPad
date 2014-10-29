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

namespace UofM.HCI.tPad.App.SurfaceViewer
{
  /// <summary>
  /// Interaction logic for SurfaceViewerApp.xaml
  /// </summary>
  public partial class SurfaceViewerApp : UserControl, ITPadApp, INotifyPropertyChanged
  {

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

    private Object captureLock = new Object();
    private Bitmap capture = null;

    public SurfaceViewerApp(TPadCore core, ITPadAppContainer container, ITPadAppController controller, Guid appUUID)
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

    private IntPtr hBitmap;
    void Registration_OnNotifyContextServiceListeners(object sender, Ubicomp.Utils.NET.CAF.ContextService.NotifyContextServiceListenersEventArgs e)
    {
      UofM.HCI.tPad.Services.RegistrationService registration = sender as UofM.HCI.tPad.Services.RegistrationService;
      if (registration.Tracker == null)
        return;

      Dispatcher.Invoke(DispatcherPriority.Render,
        (Action)delegate()
        {
          lock (captureLock)
          {
            capture = (Bitmap)registration.Tracker.GetCameraImg(true).Clone();
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

    [DllImport("gdi32.dll")]
    private static extern bool DeleteObject(IntPtr hObject);

  }
}
