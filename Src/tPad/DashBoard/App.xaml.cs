﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using UofM.HCI.tPad;
using UofM.HCI.tPad.App.Dashboard.Properties;

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
        AppClass = typeof(DashboardApp),
        Launcher = this
      };
    }

    public ITPadApp GetAppInstance(UofM.HCI.tPad.TPadApplicationDescriptor descriptor, ITPadAppContainer container, ITPadAppController controller, TPadCore core, TPadLauncherSettings settings)
    {
      DashboardApp dashboard = new DashboardApp(core, container, controller);

      Calculator.Launcher calculatorL = new Calculator.Launcher();
      dashboard.Applications.Add(calculatorL.GetApplicationDescriptor());

      ActiveReader.Launcher arL = new ActiveReader.Launcher();
      dashboard.Applications.Add(arL.GetApplicationDescriptor());

      return dashboard;
    }
  }
}
