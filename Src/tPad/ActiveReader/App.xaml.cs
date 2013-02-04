using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.IO;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.ComponentModel;

namespace UofM.HCI.tPab.App.ActiveReader
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application, ITPadAppLauncher
  {
    private SplashScreen splash = null;
    private Simulator simulatorWindow = null;

    private ActiveReaderDocument document = null;
    private TPadProfile profile = null;
    private ObservableCollection<Figure> listOfFigures = null;

    protected override void OnStartup(StartupEventArgs e)
    {
      base.OnStartup(e);

      //Launch the splash animation
      splash = new SplashScreen();
      splash.Show();

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
      CalculateFigurePositions();
    }

    private void CalculateFigurePositions()
    {
      if (listOfFigures.Count == 0)
        return;
      if (document.HasFigureLinks)
        return;

      index = 0;
      BackgroundWorker worker = new BackgroundWorker() { WorkerReportsProgress = true };
      worker.DoWork += new DoWorkEventHandler(worker_DoWork);
      worker.ProgressChanged += new ProgressChangedEventHandler(worker_ProgressChanged);
      worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
      worker.RunWorkerAsync();
    }

    private int index = 0;
    void worker_DoWork(object sender, DoWorkEventArgs e)
    {
      BackgroundWorker worker = sender as BackgroundWorker;

      double pageWidth = (profile.Resolution.Width / profile.ScreenSize.Width) * profile.DocumentSize.Width;
      double pageHeight = (profile.Resolution.Height / profile.ScreenSize.Height) * profile.DocumentSize.Height;
      if (pageWidth == 0 || pageHeight == 0)
        return;

      PDFContentHelper pdfHelper = new PDFContentHelper(document.FileName);

      //Search for the term "figure" in document
      foreach (Figure figure in listOfFigures)
      {
        List<ContentLocation> linksForFigure = pdfHelper.ContentToPixel(figure.TriggerText[1], -1, pageWidth, pageHeight);
        worker.ReportProgress(++index * 100 / listOfFigures.Count, new Object[] { figure, linksForFigure });
      }
    }

    void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
    {
      splash.LoadingProgress = e.ProgressPercentage;
      Object[] userState = (Object[])e.UserState;

      //This code is executed here because it needs to be on an UI thread
      Figure figure = (Figure)userState[0];
      List<ContentLocation> linksForFigure = (List<ContentLocation>)userState[1];
      foreach (ContentLocation figureLink in linksForFigure)
      {
        Highlight link = new Highlight();
        link.Line = new Line() { Stroke = Brushes.Yellow, Opacity = 0.7, StrokeThickness = figureLink.ContentBounds.Height, Tag = figure };
        link.Line.X1 = figureLink.ContentBounds.Left;
        link.Line.Y1 = figureLink.ContentBounds.Top + figureLink.ContentBounds.Height / 2;
        link.Line.X2 = figureLink.ContentBounds.Right;
        link.Line.Y2 = figureLink.ContentBounds.Top + figureLink.ContentBounds.Height / 2;
        document[figureLink.PageIndex].FigureLinks.Add(link);
      }
    }

    void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
      //Opens the simulator
      simulatorWindow = new Simulator(this, profile, document);
      simulatorWindow.Show();

      //Closes the splash
      splash.Close();
    }

    public ActiveReaderDocument LoadDocument(string documentFolder, string pdfFile)
    {
      if (documentFolder == null || documentFolder.Length == 0)
        throw new ArgumentException("Parameter 'documentFolders' cannot be empty");
      if (!Directory.Exists(documentFolder))
        throw new ArgumentException(String.Format("Folder '{0}' does not exist!", documentFolder[0]));

      ActiveReaderDocument result = new ActiveReaderDocument() { ID = 0, Folder = documentFolder, FileName = pdfFile };
      String[] pages = Directory.GetFiles(documentFolder, "*.png");
      Array.Sort<String>(pages);
      result.Pages = new TPadPage[pages.Length];
      for (int index = 0; index < pages.Length; index++)
        result.Pages[index] = new ActiveReaderPage() { PageIndex = index, FileName = pages[index] };

      return result;
    }

    public ITPadApp GetAppInstance(ITPadAppContainer container, ITPadAppController controller, String boardPort, String cameraPort, int deviceID)
    {
      TPadCore core = new TPadCore();
      core.BoardCOM = boardPort;
      core.CameraCOM = cameraPort;
      core.Configure(profile, deviceID);
      core.CoreStart(container, controller);

      ActiveReaderApp appInstance = new ActiveReaderApp(core, container, controller, listOfFigures);
      appInstance.DbDocuments.Add(document.ID, document.Clone() as ActiveReaderDocument);

      return appInstance;
    }
  }
}