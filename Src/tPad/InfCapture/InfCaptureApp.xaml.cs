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

namespace UofM.HCI.tPad.App.InfCapture
{
  /// <summary>
  /// Interaction logic for UserControl1.xaml
  /// </summary>
  public partial class InfCaptureApp : UserControl, ITPadApp, INotifyPropertyChanged
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

    public InfCaptureApp(TPadCore core, ITPadAppContainer container, ITPadAppController controller, Guid appUUID)
    {
      Core = core;
      Container = container;
      AppUUID = appUUID;

      InitializeComponent();

      Core.Registration.OnNotifyContextServiceListeners += Registration_OnNotifyContextServiceListeners;
    }

    void Registration_OnNotifyContextServiceListeners(object sender, Ubicomp.Utils.NET.CAF.ContextService.NotifyContextServiceListenersEventArgs e)
    {
      Dispatcher.Invoke(DispatcherPriority.Render,
        (Action)delegate()
        {
          UofM.HCI.tPad.Services.RegistrationService registration = sender as UofM.HCI.tPad.Services.RegistrationService;
          Bitmap capture = registration.Tracker.GetCameraImg(false);

          iCameraFeed.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
            capture.GetHbitmap(),
            IntPtr.Zero,
            Int32Rect.Empty,
            System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
        });
    }

    public void DeActivate() { }

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

    public void LoadInitContext(Dictionary<string, Object> context) { }

  }
}
