using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Windows.Threading;
using System.Collections.ObjectModel;
using TallComponents.PDF;
using System.IO;
using TallComponents.PDF.TextExtraction;

namespace UofM.HCI.tPab.App.ActiveReader
{
  /// <summary>
  /// Interaction logic for ActiveReaderApp.xaml
  /// </summary>
  public partial class ActiveReaderApp : UserControl, ITPadApp, INotifyPropertyChanged
  {

    public TPadProfile Profile { get; set; }
    public TPadDevice Device { get; set; }
    public ITPadAppContainer Container { get; set; }

    public String DocumentPath { get; set; }

    public double WidthScalingFactor { get; set; }
    public double HeightScalingFactor { get; set; }

    private int ActualPage { get; set; }
    private TPadDocument ActualDocument { get; set; }
    private Document PdfDocument { get; set; }

    public ActiveReaderApp(String documentPDF, ITPadAppContainer container = null)
    {
      Device = TPadCore.Instance.Device;
      Profile = TPadCore.Instance.Profile;

      WidthScalingFactor = 1;
      HeightScalingFactor = 1;

      ActualPage = -1;
      ActualDocument = null;

      Container = container;
      DocumentPath = documentPDF;

      //Opens and closes the PDF just to make sure it does exist and loads
      using (FileStream fileIn = new FileStream(DocumentPath, FileMode.Open, FileAccess.Read))
        PdfDocument = new Document(fileIn);

      InitializeComponent();
    }

    private void arApp_Loaded(object sender, RoutedEventArgs e)
    {
      TPadCore.Instance.Device.StackingChanged += new StackingChangedEventHandler(Device_StackingChanged);
      TPadCore.Instance.Device.FlippingChanged += new FlippingChangedEventHandler(Device_FlippingChanged);
      TPadCore.Instance.Device.RegistrationChanged += new RegistrationChangedEventHandler(Device_RegistrationChanged);
    }

    private void arApp_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      WidthScalingFactor = ActualWidth / Profile.Resolution.Width;
      HeightScalingFactor = ActualHeight / Profile.Resolution.Height;
      OnPropertyChanged("WidthScalingFactor");
      OnPropertyChanged("HeightScalingFactor");
    }

    void Device_StackingChanged(object sender, StackingEventArgs e)
    {
      throw new NotImplementedException();
    }

    void Device_FlippingChanged(object sender, FlippingEventArgs e)
    {
      throw new NotImplementedException();
    }

    void Device_RegistrationChanged(object sender, RegistrationEventArgs e)
    {
      if (e.NewLocation.Status != LocationStatus.Located)
      {
        if (ActualDocument != null)
        {
          //Saves current layers to disk
        }
        return;
      }

      if (ActualDocument == null)
      {
        //First time it comes to this document and first document
        if (e.NewLocation.Document != null)
        {
          //1- Loads the layers should they exist in disk
          ActualDocument = e.NewLocation.Document;
          LoadLayersFromDisk(ActualDocument);

          //2- Load layers for current page
          ActualPage = e.NewLocation.PageIndex;
          LoadLayersToPage(ActualDocument, ActualPage);
        }
        else
          throw new Exception("Document cannot be null");
      }
      else
      {
        //Change of document
        if (ActualDocument != e.NewLocation.Document)
        {
          //1- Saves current layers to disk
          SaveLayersToDisk(ActualDocument);

          //2- Loads the layers should they exist in disk
          ActualDocument = e.NewLocation.Document;
          LoadLayersFromDisk(ActualDocument);

          //3- Load layers for current page
          ActualPage = e.NewLocation.PageIndex;
          LoadLayersToPage(ActualDocument, ActualPage);
        }
        // Change of page
        else if (ActualPage != e.NewLocation.PageIndex)
        {
          //1- Saves current layers to disk
          SaveLayersToDisk(ActualDocument);

          //2- Load layers for current page
          ActualPage = e.NewLocation.PageIndex;
          LoadLayersToPage(ActualDocument, ActualPage);
        }
      }

      Dispatcher.Invoke(DispatcherPriority.Render,
        (Action)delegate()
        {
          trCanvas.Angle = e.NewLocation.RotationAngle * -1;
          trCanvas.CenterX = e.NewLocation.LocationPx.X + ActualWidth / 2;
          trCanvas.CenterY = e.NewLocation.LocationPx.Y + ActualHeight / 2;

          ttCanvas.X = e.NewLocation.LocationPx.X * -1;
          ttCanvas.Y = e.NewLocation.LocationPx.Y * -1;
        });
    }

    private void SaveLayersToDisk(TPadDocument ActualDocument)
    {
      Console.WriteLine("throw new NotImplementedException(SaveLayersToDisk);");
    }

    private void LoadLayersFromDisk(TPadDocument ActualDocument)
    {
      Console.WriteLine("throw new NotImplementedException(LoadLayersFromDisk);");
    }

    private void LoadLayersToPage(TPadDocument document, int pageIndex)
    {
      Dispatcher.Invoke(DispatcherPriority.Render,
        (Action)delegate()
        {
          //Unloads existing highlights
          var highlights = cHighlights.Children.OfType<Line>().ToList();
          foreach (Line highlight in highlights)
            cHighlights.Children.Remove(highlight);

          //Loads other highlights for this page
          foreach (UIElement element in document.Pages[pageIndex].Highlights)
          {
            Line highlight = (Line)element;
            highlight.MouseDown += cHighlights_MouseDown;
            highlight.MouseMove += cHighlights_MouseMove;
            highlight.MouseUp += cHighlights_MouseUp;
            cHighlights.Children.Add(highlight);
          }
        });
    }

    public event PropertyChangedEventHandler PropertyChanged;
    public void OnPropertyChanged(String name)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(name));
    }

    private bool isHighlighting = false;
    private Point lastPosition;
    private Line newHighlight;
    private void cHighlights_MouseDown(object sender, MouseButtonEventArgs e)
    {
      if (e.LeftButton == MouseButtonState.Pressed && e.RightButton == MouseButtonState.Released)
      {
        isHighlighting = true;
        lastPosition = Mouse.GetPosition(gAnchoredLayers);

        newHighlight = new Line() { Stroke = Brushes.YellowGreen, Opacity = 0.5, StrokeThickness = 10 };
        newHighlight.MouseDown += cHighlights_MouseDown;
        newHighlight.MouseMove += cHighlights_MouseMove;
        newHighlight.MouseUp += cHighlights_MouseUp;
        newHighlight.X1 = lastPosition.X;
        newHighlight.Y1 = lastPosition.Y;
        newHighlight.X2 = lastPosition.X;
        newHighlight.Y2 = lastPosition.Y;
        cHighlights.Children.Add(newHighlight);
      }
    }

    private void cHighlights_MouseUp(object sender, MouseButtonEventArgs e)
    {
      if (!isHighlighting)
        return;

      isHighlighting = false;
      Point newPosition = Mouse.GetPosition(gAnchoredLayers);
      newHighlight.X2 = newPosition.X;
      newHighlight.Y2 = newPosition.Y;

      Vector lineVector = new Vector(newHighlight.X2 - newHighlight.X1, newHighlight.Y2 - newHighlight.Y1);
      if (lineVector.Length > 3)
        ActualDocument.Pages[ActualPage].Highlights.Add(newHighlight);
      else //It was just a click to bring up the contextual menu
      {
        cHighlights.Children.Remove(newHighlight);
        ShowContextualMenu(newPosition);
      }
    }

    private void cHighlights_MouseMove(object sender, MouseEventArgs e)
    {
      if (!isHighlighting)
        return;

      Point newPosition = Mouse.GetPosition(gAnchoredLayers);
      newHighlight.X2 = newPosition.X;
      newHighlight.Y2 = newPosition.Y;
    }

    private void gFixedLayers_MouseDown(object sender, MouseButtonEventArgs e)
    {
      lastPosition = Mouse.GetPosition(sender as Grid);
      Console.WriteLine(lastPosition);
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
      MessageBox.Show("Hello World!");
    }

    private void ShowContextualMenu(Point position)
    {
      using (FileStream fileIn = new FileStream(DocumentPath, FileMode.Open, FileAccess.Read))
      {
        PdfDocument = new Document(fileIn);

        //1- try to find the piece of content the mouse is hovering
        TallComponents.PDF.Page page = PdfDocument.Pages[ActualPage];

        //retrieve all glyphs from the current page
        //Notice that you grep a strong reference to the glyphs, otherwise the GC can decide to recycle. 
        GlyphCollection glyphs = page.Glyphs;

        //default the glyph collection is ordered as they are present in the PDF file.
        //we want them in reading order.
        glyphs.Sort();

        foreach (Glyph glyph in glyphs)
        {
          Rect bounds = new Rect(
            glyph.TopLeft.X,
            gAnchoredLayers.ActualHeight - glyph.TopLeft.Y, 
            glyph.TopRight.X - glyph.TopLeft.X,
            glyph.TopLeft.Y - glyph.BottomLeft.Y);

          if (!bounds.Contains(position))
            continue;

          string chars = String.Empty;
          foreach (char ch in glyph.Characters)
            chars += ch.ToString();
          Console.WriteLine("{0} -[{1},{2},{3},{4}] Font={5}({6})", chars, glyph.BottomLeft,
            glyph.BottomRight, glyph.TopLeft, glyph.TopRight, glyph.Font.Name, glyph.FontSize);
        }
      }
    }

  }

}
