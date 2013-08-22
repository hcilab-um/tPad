using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using UofM.HCI.tPad;
using UofM.HCI.tPad.App.Dashboard.Properties;
using Ubicomp.Utils.NET.MTF;
using UofM.HCI.tPad.App.InfSeeking;

namespace UofM.HCI.tPad.App.Dashboard
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application, ITPadAppLauncher
  {

    private Simulator simulatorWindow = null;
    private TPadProfile profile = null;

    protected override void OnStartup(StartupEventArgs e)
    {
      base.OnStartup(e);

      //Profile of the device and surface in question
      profile = new TPadProfile()
      {
        Resolution = new Size(480, 800),
        ScreenSize = new Size(8.6, 15.4),
        DeviceSize = new Size(12.6, 18.7),
      };

      //Opens the simulator
      simulatorWindow = new Simulator(this, profile);
      simulatorWindow.Show();
    }

    public TPadLauncherSettings GetSettings(TPadLauncherSettings settings)
    {
      settings.MulticastGroup = Settings.Default.MulticastGroup;
      settings.MulticastPort = Settings.Default.MulticastPort;
      settings.MulticastTTL = Settings.Default.MulticastTTL;

      return settings;
    }

    public UofM.HCI.tPad.TPadApplicationDescriptor GetApplicationDescriptor()
    {
      return new TPadApplicationDescriptor()
      {
        Name = "Dashboard",
        Icon = null,
        AppType = typeof(DashboardApp),
        AppUUID = Guid.NewGuid(),
        Launcher = this
      };
    }

    public ITPadApp GetAppInstance(UofM.HCI.tPad.TPadApplicationDescriptor descriptor, ITPadAppContainer container, ITPadAppController controller, TPadCore core, TPadLauncherSettings settings)
    {
      DashboardApp dashboard = new DashboardApp(core, container, controller, descriptor);

      Browser.Launcher browserL = new Browser.Launcher();
      dashboard.Applications.Add(browserL.GetApplicationDescriptor());
      dashboard.DefaultFlippingAppDescriptor = dashboard.Applications[0];

      Calculator.Launcher calculatorL = new Calculator.Launcher();
      dashboard.Applications.Add(calculatorL.GetApplicationDescriptor());

      Ruler.Launcher rulerL = new Ruler.Launcher();
      dashboard.Applications.Add(rulerL.GetApplicationDescriptor());

      GraphExplorer.Launcher explorerL = new GraphExplorer.Launcher();
      dashboard.Applications.Add(explorerL.GetApplicationDescriptor());

      Tracer.Launcher tracerL = new Tracer.Launcher();
      dashboard.Applications.Add(tracerL.GetApplicationDescriptor());

      PhotoAlbum.Launcher photoAlbumL = new PhotoAlbum.Launcher();
      dashboard.Applications.Add(photoAlbumL.GetApplicationDescriptor());

      WhatUp.Launcher whatUpL = new WhatUp.Launcher();
      dashboard.Applications.Add(whatUpL.GetApplicationDescriptor());

      //Journal - Simple surface capture

      ActiveReader.Launcher arL = new ActiveReader.Launcher();
      dashboard.Applications.Add(arL.GetApplicationDescriptor());

      //Experiment 1 - Information Seeking
      SetUpExperiment1(dashboard);

      //Prepares applications for runtime
      foreach (TPadApplicationDescriptor appDesc in dashboard.Applications)
        appDesc.Launcher.Prepare();

      //Registers the dashboard as the pre-delivery listeners with the TransportComponent
      TransportComponent.Instance.PreDeliveryListener = dashboard;

      return dashboard;
    }

    private void SetUpExperiment1(DashboardApp dashboard)
    {
      InfSeeking.SeekLauncher ifL = new InfSeeking.SeekLauncher();
      dashboard.Applications.Add(ifL.GetApplicationDescriptor());

      //BLUE INFORMATION PROVIDERS - 7
      dashboard.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Blue, 1).GetApplicationDescriptor());
      dashboard.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Blue, 2).GetApplicationDescriptor());
      dashboard.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Blue, 3).GetApplicationDescriptor());
      dashboard.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Blue, 4).GetApplicationDescriptor());
      dashboard.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Blue, 5).GetApplicationDescriptor());
      dashboard.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Blue, 6).GetApplicationDescriptor());
      dashboard.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Blue, 7).GetApplicationDescriptor());

      //GREEN INFORMATION PROVIDERS -InfSeeking
      dashboard.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Green, 1).GetApplicationDescriptor());
      dashboard.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Green, 2).GetApplicationDescriptor());
      dashboard.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Green, 3).GetApplicationDescriptor());
      dashboard.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Green, 4).GetApplicationDescriptor());
      dashboard.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Green, 5).GetApplicationDescriptor());
      dashboard.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Green, 6).GetApplicationDescriptor());
      dashboard.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Green, 7).GetApplicationDescriptor());
      dashboard.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Green, 8).GetApplicationDescriptor());
      dashboard.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Green, 9).GetApplicationDescriptor());
      dashboard.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Green, 10).GetApplicationDescriptor());
      dashboard.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Green, 11).GetApplicationDescriptor());
      dashboard.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Green, 12).GetApplicationDescriptor());
      dashboard.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Green, 13).GetApplicationDescriptor());
      dashboard.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Green, 14).GetApplicationDescriptor());
      dashboard.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Green, 15).GetApplicationDescriptor());
      dashboard.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Green, 16).GetApplicationDescriptor());

      //YELLOW INFORMATION PROVIDERS InfSeeking
      dashboard.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Yellow, 1).GetApplicationDescriptor());
      dashboard.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Yellow, 2).GetApplicationDescriptor());
      dashboard.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Yellow, 3).GetApplicationDescriptor());
      dashboard.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Yellow, 4).GetApplicationDescriptor());
      dashboard.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Yellow, 5).GetApplicationDescriptor());
      dashboard.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Yellow, 6).GetApplicationDescriptor());
      dashboard.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Yellow, 7).GetApplicationDescriptor());
      dashboard.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Yellow, 8).GetApplicationDescriptor());
      dashboard.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Yellow, 9).GetApplicationDescriptor());
      dashboard.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Yellow, 10).GetApplicationDescriptor());
      dashboard.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Yellow, 11).GetApplicationDescriptor());
      dashboard.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Yellow, 12).GetApplicationDescriptor());
      dashboard.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Yellow, 13).GetApplicationDescriptor());
      dashboard.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Yellow, 14).GetApplicationDescriptor());
      dashboard.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Yellow, 15).GetApplicationDescriptor());
      dashboard.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Yellow, 16).GetApplicationDescriptor());

      //RED INFORMATION PROVIDERS - 1InfSeeking
      dashboard.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Red, 1).GetApplicationDescriptor());
      dashboard.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Red, 2).GetApplicationDescriptor());
      dashboard.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Red, 3).GetApplicationDescriptor());
      dashboard.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Red, 4).GetApplicationDescriptor());
      dashboard.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Red, 5).GetApplicationDescriptor());
      dashboard.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Red, 6).GetApplicationDescriptor());
      dashboard.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Red, 7).GetApplicationDescriptor());
      dashboard.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Red, 8).GetApplicationDescriptor());
      dashboard.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Red, 9).GetApplicationDescriptor());
      dashboard.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Red, 10).GetApplicationDescriptor());
      dashboard.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Red, 11).GetApplicationDescriptor());
      dashboard.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Red, 12).GetApplicationDescriptor());
      dashboard.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Red, 13).GetApplicationDescriptor());
      dashboard.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Red, 14).GetApplicationDescriptor());
      dashboard.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Red, 15).GetApplicationDescriptor());
      dashboard.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Red, 16).GetApplicationDescriptor());

      List<InfSeekingCondition> conditions = new List<InfSeekingCondition>();
      conditions.Add(new InfSeekingCondition(SwitchingMethod.Flipping, 1));
      conditions.Add(new InfSeekingCondition(SwitchingMethod.Flipping, 2));
      conditions.Add(new InfSeekingCondition(SwitchingMethod.Flipping, 3));
      conditions.Add(new InfSeekingCondition(SwitchingMethod.TapNFlip, 1));
      conditions.Add(new InfSeekingCondition(SwitchingMethod.TapNFlip, 2));
      conditions.Add(new InfSeekingCondition(SwitchingMethod.TapNFlip, 3));
      conditions.Add(new InfSeekingCondition(SwitchingMethod.Home, 1));
      conditions.Add(new InfSeekingCondition(SwitchingMethod.Home, 2));
      conditions.Add(new InfSeekingCondition(SwitchingMethod.Home, 3));
      conditions.Add(new InfSeekingCondition(SwitchingMethod.RuntimeBar, 1));
      conditions.Add(new InfSeekingCondition(SwitchingMethod.RuntimeBar, 2));
      conditions.Add(new InfSeekingCondition(SwitchingMethod.RuntimeBar, 3));

      CalculatePairs(conditions, 3, 1);
      dashboard.SetInfSeekingExperiment(conditions);
    }

    private Random generator = new Random((int)(DateTime.Now.Ticks % 54695));
    private void CalculatePairs(List<InfSeekingCondition> conditions, int maxDisplayDistance = 3, int trialsPerCondition = 8)
    {
      foreach (InfSeekingCondition condition in conditions)
      {
        condition.Pairs = new List<Exp1Target>();
        for (int trial = 0; trial < trialsPerCondition; trial++)
        {
          List<Exp1SourceApp> apps = new List<Exp1SourceApp>();
          for (int appIndex = 0; appIndex < condition.AppsNumber; appIndex++)
          {
            apps.Add(new Exp1SourceApp() { SourceGroup = (ProviderGroup)generator.Next(4), InstanceNro = generator.Next(7) + 1 });
            apps[appIndex].ImagePath = String.Format(@"{0}\Images\InfProvider{1}{2}.png", Environment.CurrentDirectory, apps[appIndex].SourceGroup, apps[appIndex].InstanceNro);
          }

          for (int selection = 0; selection < 3; selection++)
          {
            Exp1Target pair = new Exp1Target();
            pair.Target = generator.Next(1000);
            pair.SourceApp = apps[generator.Next(condition.AppsNumber)];
            pair.Condition = condition;
            condition.Pairs.Add(pair);
          }
        }
      }
    }

    public void Prepare() { }

  }
}
