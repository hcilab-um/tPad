using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using UofM.HCI.tPad;
using UofM.HCI.tPad.App.Shell.Properties;
using Ubicomp.Utils.NET.MTF;
using UofM.HCI.tPad.App.InfSeeking;
using UofM.HCI.tPad.App.InfCapture;

namespace UofM.HCI.tPad.App.Shell
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
        AppType = typeof(ShellApp),
        AppUUID = Guid.NewGuid(),
        Launcher = this
      };
    }

    private int[] experimentalOrder = { 10,	11,	9,	12,	8,	1,	7,	2,	6,	3,	5,	4 };

    public ITPadApp GetAppInstance(UofM.HCI.tPad.TPadApplicationDescriptor descriptor, ITPadAppContainer container, ITPadAppController controller, TPadCore core, TPadLauncherSettings settings)
    {
      ShellApp shell = new ShellApp(core, container, controller, descriptor);

      //Demo Mode - This is all the smaller apps that showcase the possibilities with a tPad
      SetDemoMode(shell);

      //Experiment 2 - Information Capture
      //SetUpExperiment2(shell, true);

      //Experiment 1 - Information Seeking
      //SetUpExperiment1(shell, true);

      //Prepares applications for runtime
      foreach (TPadApplicationDescriptor appDesc in shell.Applications)
            appDesc.Launcher.Prepare();

          //Registers the dashboard as the pre-delivery listeners with the TransportComponent
      TransportComponent.Instance.PreDeliveryListener = shell;

      return shell;
    }

    private static void SetDemoMode(ShellApp shell)
    {
      Browser.Launcher browserL = new Browser.Launcher();
      shell.Applications.Add(browserL.GetApplicationDescriptor());
      shell.DefaultFlippingAppDescriptor = shell.Applications[0];

      Calculator.Launcher calculatorL = new Calculator.Launcher();
      shell.Applications.Add(calculatorL.GetApplicationDescriptor());

      Ruler.Launcher rulerL = new Ruler.Launcher();
      shell.Applications.Add(rulerL.GetApplicationDescriptor());

      GraphExplorer.Launcher explorerL = new GraphExplorer.Launcher();
      shell.Applications.Add(explorerL.GetApplicationDescriptor());

      Tracer.Launcher tracerL = new Tracer.Launcher();
      shell.Applications.Add(tracerL.GetApplicationDescriptor());

      PhotoAlbum.Launcher photoAlbumL = new PhotoAlbum.Launcher();
      shell.Applications.Add(photoAlbumL.GetApplicationDescriptor());

      WhatUp.Launcher whatUpL = new WhatUp.Launcher();
      shell.Applications.Add(whatUpL.GetApplicationDescriptor());

      SurfaceCapture.Launcher sCaptureL = new SurfaceCapture.Launcher();
      shell.Applications.Add(sCaptureL.GetApplicationDescriptor());

      SurfaceViewer.Launcher sViewerL = new SurfaceViewer.Launcher();
      shell.Applications.Add(sViewerL.GetApplicationDescriptor());

      QReader.Launcher qrReaderL = new QReader.Launcher();
      shell.Applications.Add(qrReaderL.GetApplicationDescriptor());

      ActiveReader.Launcher arL = new ActiveReader.Launcher();
      shell.Applications.Add(arL.GetApplicationDescriptor());
    }

    private void SetUpExperiment1(ShellApp shell, bool demo)
    {
      InfSeeking.SeekLauncher ifL = new InfSeeking.SeekLauncher();
      shell.Applications.Add(ifL.GetApplicationDescriptor());

      //BLUE INFORMATION PROVIDERS - 7
      shell.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Blue, 1).GetApplicationDescriptor());
      shell.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Blue, 2).GetApplicationDescriptor());
      shell.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Blue, 3).GetApplicationDescriptor());
      shell.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Blue, 4).GetApplicationDescriptor());
      shell.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Blue, 5).GetApplicationDescriptor());
      shell.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Blue, 6).GetApplicationDescriptor());
      shell.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Blue, 7).GetApplicationDescriptor());
      shell.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Blue, 8).GetApplicationDescriptor());
      shell.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Blue, 9).GetApplicationDescriptor());
      shell.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Blue, 10).GetApplicationDescriptor());
      shell.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Blue, 11).GetApplicationDescriptor());
      shell.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Blue, 12).GetApplicationDescriptor());
      shell.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Blue, 13).GetApplicationDescriptor());
      //shell.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Blue, 14).GetApplicationDescriptor());
      //shell.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Blue, 15).GetApplicationDescriptor());

      //GREEN INFORMATION PROVIDERS -InfSeeking
      shell.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Green, 1).GetApplicationDescriptor());
      shell.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Green, 2).GetApplicationDescriptor());
      shell.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Green, 3).GetApplicationDescriptor());
      shell.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Green, 4).GetApplicationDescriptor());
      shell.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Green, 5).GetApplicationDescriptor());
      shell.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Green, 6).GetApplicationDescriptor());
      shell.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Green, 7).GetApplicationDescriptor());
      shell.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Green, 8).GetApplicationDescriptor());
      shell.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Green, 9).GetApplicationDescriptor());
      shell.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Green, 10).GetApplicationDescriptor());
      shell.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Green, 11).GetApplicationDescriptor());
      shell.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Green, 12).GetApplicationDescriptor());
      shell.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Green, 13).GetApplicationDescriptor());
      shell.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Green, 14).GetApplicationDescriptor());
      shell.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Green, 15).GetApplicationDescriptor());
      shell.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Green, 16).GetApplicationDescriptor());

      //YELLOW INFORMATION PROVIDERS InfSeeking
      shell.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Yellow, 1).GetApplicationDescriptor());
      shell.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Yellow, 2).GetApplicationDescriptor());
      shell.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Yellow, 3).GetApplicationDescriptor());
      shell.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Yellow, 4).GetApplicationDescriptor());
      shell.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Yellow, 5).GetApplicationDescriptor());
      shell.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Yellow, 6).GetApplicationDescriptor());
      shell.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Yellow, 7).GetApplicationDescriptor());
      shell.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Yellow, 8).GetApplicationDescriptor());
      shell.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Yellow, 9).GetApplicationDescriptor());
      shell.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Yellow, 10).GetApplicationDescriptor());
      shell.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Yellow, 11).GetApplicationDescriptor());
      shell.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Yellow, 12).GetApplicationDescriptor());
      shell.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Yellow, 13).GetApplicationDescriptor());
      shell.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Yellow, 14).GetApplicationDescriptor());
      shell.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Yellow, 15).GetApplicationDescriptor());
      shell.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Yellow, 16).GetApplicationDescriptor());

      //RED INFORMATION PROVIDERS - 1InfSeeking
      shell.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Red, 1).GetApplicationDescriptor());
      shell.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Red, 2).GetApplicationDescriptor());
      shell.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Red, 3).GetApplicationDescriptor());
      shell.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Red, 4).GetApplicationDescriptor());
      shell.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Red, 5).GetApplicationDescriptor());
      shell.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Red, 6).GetApplicationDescriptor());
      shell.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Red, 7).GetApplicationDescriptor());
      shell.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Red, 8).GetApplicationDescriptor());
      shell.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Red, 9).GetApplicationDescriptor());
      shell.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Red, 10).GetApplicationDescriptor());
      shell.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Red, 11).GetApplicationDescriptor());
      shell.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Red, 12).GetApplicationDescriptor());
      shell.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Red, 13).GetApplicationDescriptor());
      shell.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Red, 14).GetApplicationDescriptor());
      shell.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Red, 15).GetApplicationDescriptor());
      shell.Applications.Add(new InfSeeking.ProviderLauncher(InfSeeking.ProviderGroup.Red, 16).GetApplicationDescriptor());

      List<InfSeekingCondition> conditions = new List<InfSeekingCondition>();

      if (demo)
      {
        conditions.Add(new InfSeekingCondition(SwitchingMethod.Home, 1)); // 1
        conditions.Add(new InfSeekingCondition(SwitchingMethod.RuntimeBar, 1)); // 4
        conditions.Add(new InfSeekingCondition(SwitchingMethod.Flipping, 1)); // 7
        conditions.Add(new InfSeekingCondition(SwitchingMethod.TapNFlip, 1)); // 10
        CalculateExp1Targets(conditions, 3, 1);
      }
      else
      {
        conditions.Add(new InfSeekingCondition(SwitchingMethod.Home, 1)); // 1
        conditions.Add(new InfSeekingCondition(SwitchingMethod.Home, 2)); // 2
        conditions.Add(new InfSeekingCondition(SwitchingMethod.Home, 3)); // 3
        conditions.Add(new InfSeekingCondition(SwitchingMethod.RuntimeBar, 1)); // 4
        conditions.Add(new InfSeekingCondition(SwitchingMethod.RuntimeBar, 2)); // 5
        conditions.Add(new InfSeekingCondition(SwitchingMethod.RuntimeBar, 3)); // 6
        conditions.Add(new InfSeekingCondition(SwitchingMethod.Flipping, 1)); // 7
        conditions.Add(new InfSeekingCondition(SwitchingMethod.Flipping, 2)); // 8
        conditions.Add(new InfSeekingCondition(SwitchingMethod.Flipping, 3)); // 9
        conditions.Add(new InfSeekingCondition(SwitchingMethod.TapNFlip, 1)); // 10
        conditions.Add(new InfSeekingCondition(SwitchingMethod.TapNFlip, 2)); // 11
        conditions.Add(new InfSeekingCondition(SwitchingMethod.TapNFlip, 3)); // 12
        CalculateExp1Targets(conditions, 3, 6);
      }

      shell.SetInfSeekingExperiment(conditions, experimentalOrder);
    }

    private void SetUpExperiment2(ShellApp shell, bool demo)
    {
      SurfaceViewer.Launcher sViewerL = new SurfaceViewer.Launcher();
      shell.Applications.Add(sViewerL.GetApplicationDescriptor());

      InfCapture.Launcher icL = new InfCapture.Launcher();
      shell.Applications.Add(icL.GetApplicationDescriptor());

      List<Exp2Condition> conditions = new List<Exp2Condition>();
      if (demo)
      {
        conditions.Add(new Exp2Condition() { Device = Device.Normal, PictureMode = PictureMode.Normal, TargetSize = TargetSize.Half });
        conditions.Add(new Exp2Condition() { Device = Device.Normal, PictureMode = PictureMode.Clipped, TargetSize = TargetSize.Half });
        conditions.Add(new Exp2Condition() { Device = Device.tPad, PictureMode = PictureMode.Normal, TargetSize = TargetSize.Half });
        conditions.Add(new Exp2Condition() { Device = Device.tPad, PictureMode = PictureMode.Clipped, TargetSize = TargetSize.Half });
      }
      else
      {
        conditions.Add(new Exp2Condition() { Device = Device.Normal, PictureMode = PictureMode.Normal, TargetSize = TargetSize.Quarter });
        conditions.Add(new Exp2Condition() { Device = Device.Normal, PictureMode = PictureMode.Normal, TargetSize = TargetSize.Half });
        conditions.Add(new Exp2Condition() { Device = Device.Normal, PictureMode = PictureMode.Normal, TargetSize = TargetSize.ThreeQuarters });
        conditions.Add(new Exp2Condition() { Device = Device.Normal, PictureMode = PictureMode.Clipped, TargetSize = TargetSize.Quarter });
        conditions.Add(new Exp2Condition() { Device = Device.Normal, PictureMode = PictureMode.Clipped, TargetSize = TargetSize.Half });
        conditions.Add(new Exp2Condition() { Device = Device.Normal, PictureMode = PictureMode.Clipped, TargetSize = TargetSize.ThreeQuarters });
        conditions.Add(new Exp2Condition() { Device = Device.tPad, PictureMode = PictureMode.Normal, TargetSize = TargetSize.Quarter });
        conditions.Add(new Exp2Condition() { Device = Device.tPad, PictureMode = PictureMode.Normal, TargetSize = TargetSize.Half });
        conditions.Add(new Exp2Condition() { Device = Device.tPad, PictureMode = PictureMode.Normal, TargetSize = TargetSize.ThreeQuarters });
        conditions.Add(new Exp2Condition() { Device = Device.tPad, PictureMode = PictureMode.Clipped, TargetSize = TargetSize.Quarter });
        conditions.Add(new Exp2Condition() { Device = Device.tPad, PictureMode = PictureMode.Clipped, TargetSize = TargetSize.Half });
        conditions.Add(new Exp2Condition() { Device = Device.tPad, PictureMode = PictureMode.Clipped, TargetSize = TargetSize.ThreeQuarters });
      }

      icL.SetInfCaptureExperiment(conditions, experimentalOrder);
    }

    private Random generator = new Random((int)(DateTime.Now.Ticks % 54695));
    private void CalculateExp1Targets(List<InfSeekingCondition> conditions, int maxDisplayDistance = 3, int trialsPerCondition = 8)
    {
      foreach (InfSeekingCondition condition in conditions)
      {
        condition.Targets = new List<Exp1Target>();
        for (int trial = 0; trial < trialsPerCondition; trial++)
        {
          List<Exp1SourceApp> apps = new List<Exp1SourceApp>();
          for (int appIndex = 0; appIndex < condition.AppsNumber; appIndex++)
          {
            Exp1SourceApp source = new Exp1SourceApp() { SourceGroup = (ProviderGroup)generator.Next(4), InstanceNro = generator.Next(7) + 1 };
            if (!apps.Exists(app => app.SourceGroup == source.SourceGroup && app.InstanceNro == source.InstanceNro))
            {
              apps.Add(source);
              apps[appIndex].ImagePath = String.Format(@"{0}\Images\InfProvider{1}{2}.png", Environment.CurrentDirectory, apps[appIndex].SourceGroup, apps[appIndex].InstanceNro);
            }
            else
              appIndex--;
          }

          for (int selection = 0; selection < 3; selection++)
          {
            Exp1Target pair = new Exp1Target();
            pair.Target = generator.Next(1000);
            pair.SourceApp = apps[generator.Next(condition.AppsNumber)];
            pair.Condition = condition;
            condition.Targets.Add(pair);
          }
        }
      }
    }

    public void Prepare() { }

  }
}
