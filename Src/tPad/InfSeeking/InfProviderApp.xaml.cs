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

    public event Exp1EventHandler GetNextPair;
    private Exp1Pair currentPair = null;
    public Exp1Pair CurrentPair
    {
      get { return currentPair; }
      set
      {
        currentPair = value;
        OnPropertyChanged("CurrentPair");
      }
    }

    public InfProviderApp(TPadCore core, ITPadAppContainer container, ITPadAppController controller, Guid appUUID)
    {
      AppUUID = appUUID;
      InitializeComponent();
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

    public void LoadInitContext(Dictionary<string, Object> init) { }

    private void Label_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
      if (MessageBox.Show("Copy contents", "Copy+Paste", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
      {
        Clipboard.SetText((sender as Label).Content as String);
      }
    }

    private bool tapAndFlip = false;
    private Label source = null;
    private void Label_MouseDown(object sender, MouseButtonEventArgs e)
    {
      tapAndFlip = true;
      source = sender as Label;
    }

    private void Label_MouseUp(object sender, MouseButtonEventArgs e)
    {
      tapAndFlip = false;
      source = null;
    }

    public Dictionary<string, Object> Context
    {
      get
      {
        if (!tapAndFlip)
          return null;

        if (source == null || "EMPTY".Equals(source.Content as String))
          return null;

        Dictionary<string, object> context = new Dictionary<string, object>();
        context.Add("result", source.Content as String);
        return context;
      }
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
      LoadExperimentPair();
    }

    private void ipApp_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      LoadExperimentPair();
    }

    private void LoadExperimentPair()
    {
      Exp1Pair tmp = null;
      if (GetNextPair != null)
        tmp = GetNextPair(this, null);

      Guid instanceUUID = Guid.Parse(String.Format("00000000-0000-0000-000{0}-0000000000{1:D2}", (int)tmp.SourceApp.SourceGroup, tmp.SourceApp.InstanceNro));
      if (instanceUUID == AppUUID)
        CurrentPair = tmp;
      else
        CurrentPair = null;
    }

  }
}
