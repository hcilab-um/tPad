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

    private ActiveReaderApp reader = null;
    private Simulator simulatorWindow = null;
    private TPadWindow deviceWindow = null;

    protected override void OnStartup(StartupEventArgs e)
    {
      base.OnStartup(e);

      String[] pagesFolder = new String[1] { "Document/" };
      TPadProfile profile = new TPadProfile()
      {
        Resolution = new Size(480, 800),
        ScreenSize = new Size(8.6, 15.4),
        DeviceSize = new Size(12.6, 18.7),
        DocumentSize = new Size(21.59, 27.94) //US Letter - 215.9 mm × 279.4 mm
      };
      TPadCore.Instance.Configure(profile, true);
      TPadCore.Instance.Registration.LoadDocuments(pagesFolder);

      simulatorWindow = new Simulator(this);
      simulatorWindow.LoadTPadApp(new MockApp(simulatorWindow));

      //deviceWindow = new TPadWindow();
      //reader = new ActiveReaderApp(@"Document/FXPAL-PR-10-550.pdf", simulatorWindow);
      //deviceWindow.LoadTPadApp(reader);

      TPadCore.Instance.CoreStart(simulatorWindow);
      simulatorWindow.Show();
    }

  }
}
