using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.IO;
using System.Collections.ObjectModel;

namespace UofM.HCI.tPab.App.ActiveReader
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application, ITPadAppLauncher
  {

    private Simulator simulatorWindow = null;

    private TPadDocument document = null;
    private TPadProfile profile = null;
    private ObservableCollection<Figure> listOfFigures = null;

    protected override void OnStartup(StartupEventArgs e)
    {
      base.OnStartup(e);

      //list of hardcoded figures within the paper "LensMouse: Augmenting the Mouse with an Interactive Touch Display"
      listOfFigures = new ObservableCollection<Figure>();
      listOfFigures.Add(new Figure(1, 0, new Int32Rect(481, 741, 368, 186), new string[] { "Fig 1", "Figure 1", "Fig. 1" })); //figure 1
      listOfFigures.Add(new Figure(2, 2, new Int32Rect(510, 209, 306, 148), new string[] { "Fig 2", "Figure 2", "Fig. 2" })); //figure 2
      listOfFigures.Add(new Figure(3, 3, new Int32Rect(490, 593, 354, 150), new string[] { "Fig 3", "Figure 3", "Fig. 3" })); //figure 3
      listOfFigures.Add(new Figure(4, 4, new Int32Rect(489, 669, 337, 191), new string[] { "Fig 4", "Figure 4", "Fig. 4" })); //figure 4
      listOfFigures.Add(new Figure(5, 5, new Int32Rect(110, 543, 299, 205), new string[] { "Fig 5", "Figure 5", "Fig. 5" })); //figure 5
      listOfFigures.Add(new Figure(6, 5, new Int32Rect(507, 557, 317, 224), new string[] { "Fig 6", "Figure 6", "Fig. 6" })); //figure 6
      listOfFigures.Add(new Figure(7, 7, new Int32Rect(94, 73, 696, 161), new string[] { "Fig 7", "Figure 7", "Fig. 7" })); //figure 7
      listOfFigures.Add(new Figure(8, 7, new Int32Rect(501, 551, 331, 135), new string[] { "Fig 8", "Figure 8", "Fig. 8" })); //figure 8
      listOfFigures.Add(new Figure(9, 8, new Int32Rect(106, 210, 320, 123), new string[] { "Fig 9", "Figure 9", "Fig. 9" })); //figure 9    

      //Document over which the tPad is located
      document = LoadDocument("Document/", @"Document/FXPAL-PR-10-550.pdf");

      //Profile of the device and surface in question
      profile = new TPadProfile()
      {
        Resolution = new Size(480, 800),
        ScreenSize = new Size(8.6, 15.4),
        DeviceSize = new Size(12.6, 18.7),
        DocumentSize = new Size(21.59, 27.94) //US Letter - 215.9 mm × 279.4 mm
      };

      //*** Code to run on the deviceWindow ***//
      simulatorWindow = new Simulator(this, profile, document);
      simulatorWindow.Show();

      //simulatorWindow.LoadTPadApp(new MockApp(core1, simulatorWindow));
      //deviceWindow = new TPadWindow(core1, this);
      //reader = new ActiveReaderApp(@"Document/FXPAL-PR-10-550.pdf", core1, deviceWindow, ListOfFigures);
      //deviceWindow.LoadTPadApp(reader);
      //core1.CoreStart(deviceWindow, simulatorWindow, null, null);
      //deviceWindow.Show();

      //*** Code to run only on the simulatorWindow ***//
      //simulatorWindow = new Simulator(this);
      //reader = new ActiveReaderApp(@"Document/FXPAL-PR-10-550.pdf", simulatorWindow);
      //simulatorWindow.LoadTPadApp(reader);
      //TPadCore.Instance.CoreStart(simulatorWindow, simulatorWindow, null, null);
      //simulatorWindow.Show();
    }

    //public void StartCore(String boardPort, String cameraPort)
    //{
    //  core1.ConfigurePeripherals(boardPort, cameraPort);
    //}

    public TPadDocument LoadDocument(string documentFolder, string pdfFile)
    {
      if (documentFolder == null || documentFolder.Length == 0)
        throw new ArgumentException("Parameter 'documentFolders' cannot be empty");
      if (!Directory.Exists(documentFolder))
        throw new ArgumentException(String.Format("Folder '{0}' does not exist!", documentFolder[0]));

      TPadDocument result = new TPadDocument() { Folder = documentFolder, FileName = pdfFile };
      String[] pages = Directory.GetFiles(documentFolder, "*.png");
      Array.Sort<String>(pages);
      result.Pages = new TPadPage[pages.Length];
      for (int index = 0; index < pages.Length; index++)
        result.Pages[index] = new TPadPage() { PageIndex = index, FileName = pages[index] };

      return result;
    }

    public ITPadApp GetAppInstance(String boardPort, String cameraPort)
    {
      TPadCore core = new TPadCore();
      core.Configure(profile);
      core.ConfigurePeripherals(boardPort, cameraPort);

      ActiveReaderApp appInstance = new ActiveReaderApp(document.FileName, core, simulatorWindow, listOfFigures);

      return appInstance;
    }
  }
}