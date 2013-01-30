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

      //list of hardcoded figures within the paper "LensMouse: Augmenting the Mouse with an Interactive Touch Display"
      FigureList ListOfFigures = new FigureList();
      ListOfFigures.Figures.Add(new Figure(1, 0, new Rect(481, 741, 368, 186), new string[] { "Fig 1", "Figure 1", "Fig. 1" })); //figure 1
      ListOfFigures.Figures.Add(new Figure(2, 2, new Rect(510, 209, 306, 148), new string[] { "Fig 2", "Figure 2", "Fig. 2" })); //figure 2
      ListOfFigures.Figures.Add(new Figure(3, 3, new Rect(490, 593, 354, 150), new string[] { "Fig 3", "Figure 3", "Fig. 3" })); //figure 3
      ListOfFigures.Figures.Add(new Figure(4, 4, new Rect(489, 669, 337, 191), new string[] { "Fig 4", "Figure 4", "Fig. 4" })); //figure 4
      ListOfFigures.Figures.Add(new Figure(5, 5, new Rect(110, 543, 299, 205), new string[] { "Fig 5", "Figure 5", "Fig. 5" })); //figure 5
      ListOfFigures.Figures.Add(new Figure(6, 5, new Rect(507, 557, 317, 224), new string[] { "Fig 6", "Figure 6", "Fig. 6" })); //figure 6
      ListOfFigures.Figures.Add(new Figure(7, 7, new Rect(94, 73, 696, 161), new string[] { "Fig 7", "Figure 7", "Fig. 7" })); //figure 7
      ListOfFigures.Figures.Add(new Figure(8, 7, new Rect(501, 551, 331, 135), new string[] { "Fig 8", "Figure 8", "Fig. 8" })); //figure 8
      ListOfFigures.Figures.Add(new Figure(9, 8, new Rect(106, 210, 320, 123), new string[] { "Fig 9", "Figure 9", "Fig. 9" })); //figure 9    

      //*** Code to run on the deviceWindow ***//
      simulatorWindow = new Simulator(this);
      simulatorWindow.LoadTPadApp(new MockApp(simulatorWindow));
      deviceWindow = new TPadWindow(this);
      reader = new ActiveReaderApp(@"Document/FXPAL-PR-10-550.pdf", deviceWindow, ListOfFigures);
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
      if (deviceWindow != null)
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