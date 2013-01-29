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
  public partial class App : Application, ITPadAppLauncher
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
      TPadCore.Instance.Configure(profile);
      TPadCore.Instance.Registration.LoadDocuments(pagesFolder);

      //*** Code to run on the deviceWindow ***//
      simulatorWindow = new Simulator(this);
      simulatorWindow.LoadTPadApp(new MockApp(simulatorWindow));
      deviceWindow = new TPadWindow(this);
      reader = new ActiveReaderApp(@"Document/FXPAL-PR-10-550.pdf", deviceWindow);
      deviceWindow.LoadTPadApp(reader);
      StartCore(null, null); // runs without connecting to the board
      simulatorWindow.Show();
      deviceWindow.Show();

      //*** Code to run only on the simulatorWindow ***//
      //simulatorWindow = new Simulator(this);
      //reader = new ActiveReaderApp(@"Document/FXPAL-PR-10-550.pdf", simulatorWindow);
      //simulatorWindow.LoadTPadApp(reader);
      //StartCore(null, null); // runs without connecting to the board
      //simulatorWindow.Show();
    }

    public void StartCore(String boardPort, String cameraPort)
    {
      if(deviceWindow != null)
        TPadCore.Instance.CoreStart(deviceWindow, simulatorWindow, boardPort, cameraPort);
      else
        TPadCore.Instance.CoreStart(simulatorWindow, simulatorWindow, boardPort, cameraPort);
    }

    public void CloseAll(UIElement sender)
    {
      if (sender != simulatorWindow && simulatorWindow != null)
        simulatorWindow.Close();
      if (sender != deviceWindow && deviceWindow != null)
        deviceWindow.Close();
    }
  }

}
