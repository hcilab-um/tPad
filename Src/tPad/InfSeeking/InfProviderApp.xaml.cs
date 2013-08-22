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
using UofM.HCI.tPad.Controls;

namespace UofM.HCI.tPad.App.InfSeeking
{
  /// <summary>
  /// Interaction logic for InfProviderApp.xaml
  /// </summary>
  public partial class InfProviderApp : UserControl, ITPadApp, INotifyPropertyChanged
  {

    public event BoolEventHandler IsTopApp;
    public event RequestUserFocus RequestFocus;
    public event EventHandler Closed;
    public event PropertyChangedEventHandler PropertyChanged;

    public Guid AppUUID { get; private set; }
    public TPadCore Core { get; set; }
    public ITPadAppContainer Container { get; set; }
    public ITPadAppController Controller { get; set; }
    public String Image { get; set; }
    public TPadApplicationDescriptor Descriptor { get; set; }

    public event Exp1EventHandler GetNextTarget;
    private Exp1Target currentTarget = null;
    public Exp1Target CurrentTarget
    {
      get { return currentTarget; }
      set
      {
        currentTarget = value;
        OnPropertyChanged("CurrentTarget");
      }
    }

    private NotificationDialog notification = null;

    public InfProviderApp(TPadCore core, ITPadAppContainer container, ITPadAppController controller, TPadApplicationDescriptor descriptor)
    {
      Core = core;
      Container = container;
      AppUUID = descriptor.AppUUID;
      Image = descriptor.Icon as String;
      Descriptor = descriptor;

      InitializeComponent();

      notification = new NotificationDialog(Core, Guid.NewGuid());
      notification.ClickedOK += new EventHandler(notification_ClickedOK);
      notification.ClickedCancel += new EventHandler(notification_ClickedCancel);
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
      LoadExperimentPair();
    }

    private void Label_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
      if (CurrentTarget.Condition.Method == SwitchingMethod.TapNFlip)
        return;

      Dictionary<String, Object> context = new Dictionary<String, Object>();
      context.Add("message", "Copy contents?");
      context.Add("buttonOK", "Yes");
      context.Add("buttonCancel", "No");
      context.Add("sender", sender);
      context.Add("currentApp", Descriptor);

      notification.LoadInitContext(context);
      Container.LoadTPadApp(notification, true);
    }

    void notification_ClickedOK(object sender, EventArgs e)
    {
      Clipboard.SetText(CurrentTarget.Target.ToString());
    }

    void notification_ClickedCancel(object sender, EventArgs e)
    {

    }

    private bool tapAndFlip = false;
    private Label source = null;
    private DateTime timeMouseUp;
    private void Label_MouseDown(object sender, MouseButtonEventArgs e)
    {
      if (CurrentTarget.Condition.Method != SwitchingMethod.TapNFlip)
        return;

      tapAndFlip = true;
      source = sender as Label;
    }

    private void Label_MouseUp(object sender, MouseButtonEventArgs e)
    {
      tapAndFlip = false;
      timeMouseUp = DateTime.Now;
    }

    public Dictionary<string, Object> Context
    {
      get
      {
        if (!tapAndFlip && (DateTime.Now - timeMouseUp).TotalMilliseconds >= 1000)
          return null;

        if (source == null || "EMPTY".Equals(source.Content as String))
          return null;

        Dictionary<string, object> context = new Dictionary<string, object>();
        context.Add("result", source.Content.ToString());
        return context;
      }
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs ev)
    {
      LoadExperimentPair();
    }

    private void LoadExperimentPair()
    {
      Exp1Target tmp = null;
      if (GetNextTarget != null)
        tmp = GetNextTarget(this, null);

      Guid instanceUUID = Guid.Parse(String.Format("00000000-0000-0000-000{0}-0000000000{1:D2}", (int)tmp.SourceApp.SourceGroup, tmp.SourceApp.InstanceNro));
      if (instanceUUID == AppUUID)
        CurrentTarget = tmp;
      else
        CurrentTarget = null;
    }

  }
}
