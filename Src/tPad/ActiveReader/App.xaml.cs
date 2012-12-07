using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace UofM.HCI.tPab.App.ActiveReader
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application
  {

    private Window hostWindow = null;

    protected override void OnStartup(StartupEventArgs e)
    {
      base.OnStartup(e);

      hostWindow = new Simulator(this, "paper_page.png", new ActiveReaderApp());
      TPadProfile profile = new TPadProfile()
      {
        Resolution = new Size(800, 480),
        ScreenSize = new Size(8.6, 15.4),
        DeviceSize = new Size(12.6, 18.7),
        DocumentSize = new Size(21, 29.7)
      };

      TPadCore.Instance.Startup(profile, true, hostWindow);
      hostWindow.Show();
    }

  }
}
