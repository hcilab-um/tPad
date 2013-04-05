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
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Threading;

namespace UofM.HCI.tPab.Applications
{
  /// <summary>
  /// Interaction logic for Dashboard.xaml
  /// </summary>
  public partial class DashboardApp : UserControl, ITPadApp, INotifyPropertyChanged
  {

    public event EventHandler Closed;

    private TPadCore core;
    public TPadCore Core
    {
      get { return core; }
      set
      {
        core = value;
        OnPropertyChanged("Core");
      }
    }

    public TPadProfile Profile { get; set; }
    public ITPadAppContainer Container { get; set; }
    public ITPadAppController Controller { get; set; }

    public ObservableCollection<TPadApplicationDescriptor> Applications { get; private set; }

    public DashboardApp(TPadCore core, ITPadAppContainer container, ITPadAppController controller)
    {
      Core = core;
      Container = container;
      Controller = controller;
      Profile = core.Profile;

      Applications = new ObservableCollection<TPadApplicationDescriptor>();
      InitializeComponent();

      core.GlyphsChanged += core_GlyphsChanged;
    }


    public event PropertyChangedEventHandler PropertyChanged;
    public void OnPropertyChanged(String name)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(name));
    }

    private void Image_MouseUp(object sender, MouseButtonEventArgs e)
    {
      TPadApplicationDescriptor descriptor = (sender as Image).DataContext as TPadApplicationDescriptor;
      ITPadApp application = descriptor.Launcher.GetAppInstance(descriptor, Container, Controller, core, null);
      Container.LoadTPadApp(application);
    }

    public void LaunchTPadApp(Type appType)
    {
      var descriptor = Applications.SingleOrDefault(tmp => tmp.AppClass.Equals(appType));
      if (descriptor == null)
        return;

      ITPadApp application = descriptor.Launcher.GetAppInstance(descriptor, Container, Controller, core, null);
      Container.LoadTPadApp(application);
    }

    void core_GlyphsChanged(object sender, GlyphsEventArgs e)
    {
      Dispatcher.Invoke(DispatcherPriority.Render,
        (Action)delegate()
        {
          foreach (GlyphEvent gEvent in e.GlyphEvents)
          {
            var descriptor = Applications.SingleOrDefault(app => app.Triggers.Exists(glyph => glyph == gEvent.Glyph));
            if (descriptor == null)
              continue;

            if (gEvent.Status == GlyphStatus.Entered)
            {
              ITPadApp application = Container.GetRunningInstance(descriptor.AppClass);
              if (application != null)
                continue;
              core.Registration.Pause();
              application = descriptor.Launcher.GetAppInstance(descriptor, Container, Controller, core, null);
              Container.LoadTPadApp(application);
            }
            else if (gEvent.Status == GlyphStatus.Left)
            {
              ITPadApp application = Container.GetRunningInstance(descriptor.AppClass);
              if (application == null)
                continue;
              application.Close();
            }
          }
        });
    }

    public void Close()
    {
      if (Closed != null)
        Closed(this, EventArgs.Empty);
    }
  }

}
