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

    public enum ActiveReaderMode { Nothing, Highlighting, Annotating };

    public TPadProfile Profile { get; set; }
    public TPadDevice Device { get; set; }
    public ITPadAppContainer Container { get; set; }

    public String DocumentPath { get; set; }

    public double WidthScalingFactor { get; set; }
    public double HeightScalingFactor { get; set; }

    private int actualPage = -1;
    public int ActualPage
    {
      get { return actualPage; }
      set
      {
        actualPage = value;
        OnPropertyChanged("ActualPage");
      }
    }

    private TPadDocument actualDocument = null;
    public TPadDocument ActualDocument
    {
      get { return actualDocument; }
      set
      {
        actualDocument = value;
        OnPropertyChanged("ActualDocument");
      }
    }

    private Document PdfDocument { get; set; }

    private ActiveReaderMode currentMode = ActiveReaderMode.Nothing;
    public ActiveReaderMode CurrentMode
    {
      get { return currentMode; }
      set
      {
        currentMode = value;
        OnPropertyChanged("CurrentMode");
      }
    }

    private bool showPageImage = false;
    public bool ShowPageImage
    {
      get { return showPageImage; }
      set
      {
        showPageImage = value;
        OnPropertyChanged("ShowPageImage");
      }
    }

    public ActiveReaderApp(String documentPDF, ITPadAppContainer container)
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

      WidthScalingFactor = ActualWidth / Profile.Resolution.Width;
      HeightScalingFactor = ActualHeight / Profile.Resolution.Height;
      OnPropertyChanged("WidthScalingFactor");
      OnPropertyChanged("HeightScalingFactor");
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
          System.Drawing.PointF locationPx = new System.Drawing.PointF(
            e.NewLocation.LocationCm.X * Container.WidthFactor,
            e.NewLocation.LocationCm.Y * Container.HeightFactor);

          trCanvas.Angle = e.NewLocation.RotationAngle * -1;
          trCanvas.CenterX = locationPx.X + ActualWidth / 2;
          trCanvas.CenterY = locationPx.Y + ActualHeight / 2;

          ttCanvas.X = locationPx.X * -1;
          ttCanvas.Y = locationPx.Y * -1;
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
        String content = PixelToContent(newPosition, true);
        ShowContextualMenu(newPosition, content);
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
    }

    private void bHighlight_Click(object sender, RoutedEventArgs e)
    {
      if (bHighlight.IsChecked.Value)
      {
        CurrentMode = ActiveReaderMode.Highlighting;
      }
      else
      {
        CurrentMode = ActiveReaderMode.Nothing;
      }
    }

    private void bAnnotation_Click(object sender, RoutedEventArgs e)
    {
      MessageBox.Show("Annotations");
    }

    private string PixelToContent(Point position, bool highlight = false)
    {
      RemoveWordHighlight();

      using (FileStream fileIn = new FileStream(DocumentPath, FileMode.Open, FileAccess.Read))
      {
        PdfDocument = new Document(fileIn);

        //1- try to find the piece of content the mouse is hovering
        TallComponents.PDF.Page page = PdfDocument.Pages[ActualPage];

        double widthT = gAnchoredLayers.ActualWidth / page.Width;
        double heightT = gAnchoredLayers.ActualHeight / page.Height;

        //retrieve all glyphs from the current page
        //Notice that you grep a strong reference to the glyphs, otherwise the GC can decide to recycle. 
        GlyphCollection glyphs = page.Glyphs;

        //default the glyph collection is ordered as they are present in the PDF file.
        //we want them in reading order.
        glyphs.Sort();

        //the bounds of the last glyph analysed
        Rect glyphBounds = Rect.Empty;

        //the current word over which the user clicked
        StringBuilder currentWord = new StringBuilder();
        Rect wordBounds = Rect.Empty;
        bool foundWord = false;

        foreach (Glyph glyph in glyphs)
        {
          if (glyph.Characters.Length == 0 || glyph.Characters[0] == ' ')
          {
            if (foundWord)
            {
              double wordWidth = glyphBounds.Right - wordBounds.Left;
              if (wordWidth > 0) //multi-line word -- the bounds cover only the upper part of it
                wordBounds = new Rect(wordBounds.Left, wordBounds.Top, wordWidth, wordBounds.Height);

              if (highlight)
                AddWordHighlight(wordBounds);
              return currentWord.ToString();
            }

            wordBounds = Rect.Empty;
            currentWord.Clear();
            continue;
          }

          glyphBounds = new Rect(
            glyph.TopLeft.X,
            page.Height - glyph.TopLeft.Y,
            glyph.TopRight.X - glyph.TopLeft.X,
            glyph.TopLeft.Y - glyph.BottomLeft.Y);
          glyphBounds.Scale(widthT, heightT);

          if (wordBounds == Rect.Empty)
            wordBounds = glyphBounds;

          string chars = String.Empty;
          foreach (char ch in glyph.Characters)
            currentWord.Append(ch);

          if (!glyphBounds.Contains(position))
            continue;

          foundWord = true;
          //Console.WriteLine("{0} -[{1},{2},{3},{4}] Font={5}({6})", chars, glyph.BottomLeft,
          //  glyph.BottomRight, glyph.TopLeft, glyph.TopRight, glyph.Font.Name, glyph.FontSize);
        }

        return null;
      }
    }

    private void RemoveWordHighlight()
    {
      if (wordHighlight == null)
        return;
      cHighlights.Children.Remove(wordHighlight);
    }

    private Line wordHighlight;
    private void AddWordHighlight(Rect wordBounds)
    {
      wordHighlight = new Line() { Stroke = Brushes.Pink, Opacity = 0.5, StrokeThickness = wordBounds.Height };
      wordHighlight.X1 = wordBounds.Left;
      wordHighlight.Y1 = wordBounds.Top + wordBounds.Height / 2;
      wordHighlight.X2 = wordBounds.Right;
      wordHighlight.Y2 = wordBounds.Top + wordBounds.Height / 2;
      cHighlights.Children.Add(wordHighlight);
    }

    private void ShowContextualMenu(Point position, String content)
    {
    }

  }
}