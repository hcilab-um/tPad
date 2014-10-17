using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows;
using System.ComponentModel;
using System.Windows.Shapes;
using System.Windows.Media;
using System.IO;

namespace UofM.HCI.tPad.App.ActiveReader
{
  public class DocumentLoader
  {
    private SplashScreen splash = null;
    private ActiveReaderDocument document = null;
    private ObservableCollection<Figure> listOfFigures = new ObservableCollection<Figure>();

    public ActiveReaderDocument LoadDocument(TPadDocument inputDoc)
    {
      //Launch the splash animation
      //splash = new SplashScreen();
      //splash.Show();

      //Document over which the tPad is located
      document = LoadDocument(inputDoc.Folder, inputDoc.FileName, inputDoc.ID);

      //list of hardcoded figures within the paper "LensMouse: Augmenting the Mouse with an Interactive Touch Display"
      if (document.ID == 1)
      {
        listOfFigures.Add(new Figure(1, 1, new Int32Rect(93, 254, 342, 122), new string[] { "Fig 1", "Figure 1", "Fig. 1" }));
        listOfFigures.Add(new Figure(2, 1, new Int32Rect(483, 570, 362, 388), new string[] { "Fig 2", "Figure 2", "Fig. 2" }));
        listOfFigures.Add(new Figure(3, 2, new Int32Rect(93, 468, 357, 186), new string[] { "Fig 3", "Figure 3", "Fig. 3" }));
        listOfFigures.Add(new Figure(4, 2, new Int32Rect(477, 853, 367, 237), new string[] { "Fig 4", "Figure 4", "Fig. 4" }));
        listOfFigures.Add(new Figure(5, 3, new Int32Rect(92, 146, 360, 206), new string[] { "Fig 5", "Figure 5", "Fig. 5" }));
        listOfFigures.Add(new Figure(6, 3, new Int32Rect(481, 423, 362, 187), new string[] { "Fig 6", "Figure 6", "Fig. 6" }));
        listOfFigures.Add(new Figure(7, 4, new Int32Rect(487, 105, 348, 265), new string[] { "Fig 7", "Figure 7", "Fig. 7" }));
        listOfFigures.Add(new Figure(8, 5, new Int32Rect(99, 120, 348, 250), new string[] { "Fig 8", "Figure 8", "Fig. 8" }));
        listOfFigures.Add(new Figure(9, 5, new Int32Rect(87, 376, 364, 206), new string[] { "Fig 9", "Figure 9", "Fig. 9" }));
        listOfFigures.Add(new Figure(10, 5, new Int32Rect(95, 584, 348, 301), new string[] { "Fig 10", "Figure 10", "Fig. 10" }));
        listOfFigures.Add(new Figure(11, 6, new Int32Rect(93, 121, 356, 181), new string[] { "Fig 11", "Figure 11", "Fig. 11" }));
        listOfFigures.Add(new Figure(12, 6, new Int32Rect(95, 641, 353, 245), new string[] { "Fig 12", "Figure 12", "Fig. 12" }));
        listOfFigures.Add(new Figure(13, 7, new Int32Rect(98, 540, 347, 254), new string[] { "Fig 13", "Figure 13", "Fig. 13" }));
        listOfFigures.Add(new Figure(14, 7, new Int32Rect(478, 671, 362, 340), new string[] { "Fig 14", "Figure 14", "Fig. 14" }));
        listOfFigures.Add(new Figure(15, 8, new Int32Rect(95, 281, 350, 345), new string[] { "Fig 15", "Figure 15", "Fig. 15" }));
        listOfFigures.Add(new Figure(16, 8, new Int32Rect(87, 634, 361, 212), new string[] { "Fig 16", "Figure 16", "Fig. 16" }));
        listOfFigures.Add(new Figure(17, 8, new Int32Rect(481, 812, 355, 224), new string[] { "Fig 17", "Figure 17", "Fig. 17" }));
        listOfFigures.Add(new Figure(18, 9, new Int32Rect(483, 109, 355, 203), new string[] { "Fig 18", "Figure 18", "Fig. 18" }));
        listOfFigures.Add(new Figure(19, 9, new Int32Rect(482, 552, 358, 189), new string[] { "Fig 19", "Figure 19", "Fig. 19" }));
      }
      CalculateFigurePositions();
      return document;
    }

    private ActiveReaderDocument LoadDocument(string documentFolder, string pdfFile, int docID)
    {
      if (documentFolder == null || documentFolder.Length == 0)
        throw new ArgumentException("Parameter 'documentFolders' cannot be empty");
      if (!Directory.Exists(documentFolder))
        throw new ArgumentException(String.Format("Folder '{0}' does not exist!", documentFolder));

      ActiveReaderDocument result = new ActiveReaderDocument() { ID = docID, Folder = documentFolder, FileName = pdfFile };
      result.DocumentSize = new Size(21.59, 27.94); //US Letter - 215.9 mm × 279.4 mm

      String[] pages = Directory.GetFiles(documentFolder, "*.png");
      Array.Sort<String>(pages);
      result.Pages = new TPadPage[pages.Length];
      for (int index = 0; index < pages.Length; index++)
        result.Pages[index] = new ActiveReaderPage() { PageIndex = index, FileName = pages[index] };

      return result;
    }

    private void CalculateFigurePositions()
    {
      if (listOfFigures.Count == 0)
      {
        worker_RunWorkerCompleted(null, null);
        return;
      }

      PDFContentHelper pdfHelper = new PDFContentHelper(document.FileName);
      pdfHelper.LoadLayersFromDisk(document, -1);
      if (document.HasFigureLinks)
      {
        worker_RunWorkerCompleted(null, null);
        return;
      }

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

      PDFContentHelper pdfHelper = new PDFContentHelper(document.FileName);
      //Search for the term "figure" in document
      foreach (Figure figure in listOfFigures)
      {
        List<ContentLocation> linksForFigure = pdfHelper.ContentToPixel(figure.TriggerText[1], -1, document.DocumentSize.Width, document.DocumentSize.Height);
        worker.ReportProgress(++index * 100 / listOfFigures.Count, new Object[] { figure, linksForFigure });
        //worker.ReportProgress(++index * 100 / listOfFigures.Count, new Object[] { figure, new List<ContentLocation>() });
      }
    }

    void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
    {
      //splash.LoadingProgress = e.ProgressPercentage;
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
      //Saves the calculated figures
      PDFContentHelper pdfHelper = new PDFContentHelper(document.FileName);
      pdfHelper.SaveLayersToDisk(document, -1);

      //Closes the splash
      //splash.Close();
    }

  }
}
