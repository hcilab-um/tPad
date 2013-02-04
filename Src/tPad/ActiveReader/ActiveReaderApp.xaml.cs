﻿using System;
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
using System.Text.RegularExpressions;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

namespace UofM.HCI.tPab.App.ActiveReader
{
  /// <summary>
  /// Interaction logic for ActiveReaderApp.xaml
  /// </summary>
  public partial class ActiveReaderApp : UserControl, ITPadApp, INotifyPropertyChanged
  {

    public ObservableCollection<Figure> FigurePositions { get; set; }
    public Dictionary<int, ActiveReaderDocument> DbDocuments { get; set; }

    public TPadCore Core { get; set; }
    public ITPadAppContainer Container { get; set; }
    public ITPadAppController Controller { get; set; }

    public double WidthScalingFactor { get; set; }
    public double HeightScalingFactor { get; set; }

    private PDFContentHelper PdfHelper { get; set; }

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

    private ActiveReaderDocument actualDocument = null;
    public ActiveReaderDocument ActualDocument
    {
      get { return actualDocument; }
      set
      {
        actualDocument = value;
        OnPropertyChanged("ActualDocument");
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

    public float KeyboardPosition
    {
      get { return (float)(gFixedLayers.Height - tpKeyboard.Height); }
    }

    private String searchTerm = String.Empty;
    public String SearchTerm
    {
      get { return searchTerm; }
      set
      {
        searchTerm = value.ToString();
        this.OnPropertyChanged("SearchTerm");
      }
    }

    private BitmapSource croppedImage;
    public BitmapSource CroppedImage
    {
      get { return croppedImage; }
      set
      {
        croppedImage = value;
        OnPropertyChanged("CroppedImage");
      }
    }

    public ActiveReaderPage ActualPageObject
    {
      get
      {
        if (ActualDocument == null || ActualPage == -1 || ActualDocument.Pages == null || ActualDocument.Pages.Length <= ActualPage)
          return null;
        return ActualDocument[ActualPage];
      }
    }

    private Note actualNote = new Note();
    public Note ActualNote
    {
      get { return actualNote; }
      set
      {
        actualNote = value;
        this.OnPropertyChanged("ActualNote");
      }
    }

    public ActiveReaderApp(TPadCore core, ITPadAppContainer container, ITPadAppController controller, ObservableCollection<Figure> figures)
    {
      Core = core;

      ActualPage = -1;
      ActualDocument = null;

      Container = container;
      Controller = controller;

      FigurePositions = figures;
      DbDocuments = new Dictionary<int, ActiveReaderDocument>();

      WidthScalingFactor = 1;
      HeightScalingFactor = 1;

      PropertyChanged += new PropertyChangedEventHandler(ActiveReaderApp_PropertyChanged);
      InitializeComponent();
    }

    void ActiveReaderApp_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == "ActualPage" || e.PropertyName == "ActualDocument")
        OnPropertyChanged("ActualPageObject");
    }

    private void arApp_Loaded(object sender, RoutedEventArgs e)
    {
      Core.Device.StackingChanged += new StackingChangedEventHandler(Device_StackingChanged);
      Core.Device.FlippingChanged += new FlippingChangedEventHandler(Device_FlippingChanged);
      Core.Device.RegistrationChanged += new RegistrationChangedEventHandler(Device_RegistrationChanged);

      WidthScalingFactor = ActualWidth / Core.Profile.Resolution.Width;
      HeightScalingFactor = ActualHeight / Core.Profile.Resolution.Height;
      OnPropertyChanged("WidthScalingFactor");
      OnPropertyChanged("HeightScalingFactor");

      BindingOperations.SetBinding(cm_searchItem, MenuItem.HeaderProperty, new Binding("SearchTerm")
      {
        Source = this,
        Converter = new UofM.HCI.tPab.App.ActiveReader.Converters.StringToContextMenuLabelConverter(),
        ConverterParameter = "Search \"{0}\""
      });

      BindingOperations.SetBinding(cm_searchItem, MenuItem.VisibilityProperty, new Binding("SearchTerm")
      {
        Source = this,
        Converter = new UofM.HCI.tPab.App.ActiveReader.Converters.ContextMenuVisibilityConverter(),
      });
    }

    private void arApp_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      WidthScalingFactor = ActualWidth / Core.Profile.Resolution.Width;
      HeightScalingFactor = ActualHeight / Core.Profile.Resolution.Height;
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
        if (e.NewLocation.DocumentID != -1)
          LoadDocument(e.NewLocation);
        else
          throw new Exception("Document cannot be null");
      }
      else
      {
        //Change of document
        if (ActualDocument.ID != e.NewLocation.DocumentID)
        {
          //1- Saves current layers to disk
          SaveLayersToDisk(ActualDocument);

          //2- Loads the layers (for current page)
          LoadDocument(e.NewLocation);
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

    private void LoadDocument(TPadLocation newLocation)
    {
      //1- Loads the layers should they exist in disk
      ActualDocument = DbDocuments[newLocation.DocumentID];
      PdfHelper = new PDFContentHelper(ActualDocument.FileName);
      LoadLayersFromDisk(ActualDocument);

      //3- Load layers for current page
      ActualPage = newLocation.PageIndex;
      LoadLayersToPage(ActualDocument, ActualPage);
    }

    private void SaveLayersToDisk(TPadDocument document)
    {
      return;

      XmlSerializer serializer = new XmlSerializer(typeof(TPadDocument));
      TextWriter textWriter = new StreamWriter(document.Folder + "cache.xml");
      serializer.Serialize(textWriter, document);
      textWriter.Close();
    }

    private void LoadLayersFromDisk(ActiveReaderDocument document)
    {
      return;

      XmlSerializer deserializer = new XmlSerializer(typeof(TPadDocument));
      TextReader textReader = new StreamReader(document.Folder + "cache.xml");
      ActiveReaderDocument newDoc = (ActiveReaderDocument)deserializer.Deserialize(textReader);
      textReader.Close();

      for (int index = 0; index < document.Pages.Length; index++)
      {
        document[index].Annotations = newDoc[index].Annotations;
        document[index].Highlights = newDoc[index].Highlights;
        document[index].Scribblings = newDoc[index].Scribblings;
        document[index].SearchResults = newDoc[index].SearchResults;
        document[index].FigureLinks = newDoc[index].FigureLinks;
      }
    }

    private void LoadLayersToPage(ActiveReaderDocument document, int pageIndex)
    {
      Dispatcher.Invoke(DispatcherPriority.Render,
        (Action)delegate()
        {
          //Unloads existing highlights
          var highlights = cHighlights.Children.OfType<Line>().ToList();
          foreach (Line highlight in highlights)
            cHighlights.Children.Remove(highlight);

          //Unload existing notes
          var notes = cHighlights.Children.OfType<TextBox>().ToList();
          foreach (TextBox note in notes)
            cHighlights.Children.Remove(note);

          //Unload existing icons
          var icons = cHighlights.Children.OfType<Image>().ToList();
          foreach (Image icon in icons)
            cHighlights.Children.Remove(icon);

          //Unload existing scribblings
          var scribblings = cHighlights.Children.OfType<InkCanvas>().ToList();
          foreach (InkCanvas scribble in scribblings)
            cHighlights.Children.Remove(scribble);

          //Unload existing search results
          cSearchResults.Children.Clear();

          //Loads other highlights for this page
          foreach (Highlight element in document[pageIndex].Highlights)
          {
            Highlight highlight = (Highlight)element;
            highlight.Line.MouseDown += cHighlights_MouseDown;
            highlight.Line.MouseMove += cHighlights_MouseMove;
            highlight.Line.MouseUp += cHighlights_MouseUp;
            cHighlights.Children.Add(highlight.Line);
          }

          //Loads search results for this page
          foreach (Highlight element in document[pageIndex].SearchResults)
          {
            Highlight searchHighlight = (Highlight)element;
            searchHighlight.Line.MouseDown += cHighlights_MouseDown;
            searchHighlight.Line.MouseMove += cHighlights_MouseMove;
            searchHighlight.Line.MouseUp += cHighlights_MouseUp;
            cSearchResults.Children.Add(searchHighlight.Line);
          }

          //Loads figure links for this page
          foreach (Highlight element in document[pageIndex].FigureLinks)
          {
            Highlight linkHighlight = (Highlight)element;
            linkHighlight.Line.MouseDown += cHighlights_MouseDown;
            linkHighlight.Line.MouseMove += cHighlights_MouseMove;
            linkHighlight.Line.MouseUp += cHighlights_MouseUp;
            cSearchResults.Children.Add(linkHighlight.Line);
          }

          //Loads notes for this page
          foreach (Note element in document[pageIndex].Annotations)
          {
            Note note = (Note)element;
            cHighlights.Children.Add(note.Annotation);
            cHighlights.Children.Add(note.Icon);
          }

          //Loads scribbles for this page
          foreach (Scribble element in document[pageIndex].Scribblings)
          {
            Scribble note = (Scribble)element;
            cHighlights.Children.Add(note.Scribbling);
            cHighlights.Children.Add(note.Icon);
          }
        });
    }

    public event PropertyChangedEventHandler PropertyChanged;
    public void OnPropertyChanged(String name)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(name));

      if (name == "ActualPage" || name == "ActualDocument")
        OnPropertyChanged("ActualPageObject");
    }

    private void ClearSearch()
    {
      foreach (ActiveReaderPage documentPage in ActualDocument.Pages)
        documentPage.SearchResults.Clear();
      cSearchResults.Children.Clear();
    }

    private void Search(String word, int page)
    {
      ClearSearch();

      List<ContentLocation> pageSearch = PdfHelper.ContentToPixel(word, page, gAnchoredLayers.ActualWidth, gAnchoredLayers.ActualHeight);

      foreach (ContentLocation content in pageSearch)
      {
        Highlight resultHL = new Highlight();
        resultHL.Line = new Line() { Stroke = Brushes.Blue, Opacity = 0.5, StrokeThickness = content.ContentBounds.Height };
        resultHL.Line.X1 = content.ContentBounds.Left;
        resultHL.Line.Y1 = content.ContentBounds.Top + content.ContentBounds.Height / 2;
        resultHL.Line.X2 = content.ContentBounds.Right;
        resultHL.Line.Y2 = content.ContentBounds.Top + content.ContentBounds.Height / 2;
        if (content.PageIndex == ActualPage)
          cSearchResults.Children.Add(resultHL.Line);
        (ActualDocument.Pages[content.PageIndex] as ActiveReaderPage).SearchResults.Add(resultHL);
      }
    }

    private bool isHighlighting = false;
    private Point lastPosition;
    private Highlight newHighlight = new Highlight();
    private Line currentHighlight;
    private bool isSomething2Hide = false;
    private bool isSenderHighlight = false;
    private void cHighlights_MouseDown(object sender, MouseButtonEventArgs e)
    {
      if (e.LeftButton == MouseButtonState.Pressed && e.RightButton == MouseButtonState.Released)
      {
        isHighlighting = true;
        lastPosition = Mouse.GetPosition(gAnchoredLayers);

        newHighlight = new Highlight();
        newHighlight.Line = new Line { Stroke = Brushes.YellowGreen, Opacity = 0.5, StrokeThickness = 18 };
        newHighlight.Line.MouseDown += cHighlights_MouseDown;
        newHighlight.Line.MouseMove += cHighlights_MouseMove;
        newHighlight.Line.MouseUp += cHighlights_MouseUp;
        newHighlight.Line.X1 = lastPosition.X;
        newHighlight.Line.Y1 = lastPosition.Y;
        newHighlight.Line.X2 = lastPosition.X;
        newHighlight.Line.Y2 = lastPosition.Y;
        cHighlights.Children.Add(newHighlight.Line);

        contextMenu.Visibility = Visibility.Hidden;
        tpKeyboard.Visibility = Visibility.Hidden;

        isSomething2Hide = false;
        foreach (Note element in ActualDocument[ActualPage].Annotations)
        {
          if (element.Annotation.Visibility == Visibility.Visible)
          {
            isSomething2Hide = true;
            element.Annotation.Visibility = Visibility.Hidden;
          }
        }

        foreach (Scribble element in ActualDocument[ActualPage].Scribblings)
        {
          if (element.Scribbling.Visibility == Visibility.Visible)
          {
            isSomething2Hide = true;
            element.Scribbling.Visibility = Visibility.Hidden;
          }
        }

        if (sender is Line)
        {          
          Line line = (Line) sender;
          if (line.Tag != null)
          {
            isHighlighting = false; //to avoid highlighting in Figure-Mode
            GetFigure((line.Tag as Figure));
          }
          else
          {
            isSenderHighlight = true;
            currentHighlight = line;
          }
        }
        else isSenderHighlight = false;
      }
    }

    private void GetFigure(Figure figure)
    {
      System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(ActualDocument.Pages[figure.PageIndex].FileName);

      BitmapSource source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty,
        System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
      CroppedImage = new CroppedBitmap(source, figure.FigureRect);

      figureViewer.Visibility = Visibility.Visible;
    }

    private float minlength_Highlight = 10;
    private void cHighlights_MouseUp(object sender, MouseButtonEventArgs e)
    {
      if (!isHighlighting)
        return;

      isHighlighting = false;
      Point newPosition = Mouse.GetPosition(gAnchoredLayers);
      newHighlight.Line.X2 = newPosition.X;
      newHighlight.Line.Y2 = newPosition.Y;

      Vector lineVector = new Vector(newHighlight.Line.X2 - newHighlight.Line.X1, newHighlight.Line.Y2 - newHighlight.Line.Y1);
      if (lineVector.Length > minlength_Highlight)
        ActualDocument[ActualPage].Highlights.Add(newHighlight);
      else //It was just a click to bring up the contextual menu
      {
        cHighlights.Children.Remove(newHighlight.Line);

        Rect contentBounds = Rect.Empty;
        String content = PdfHelper.PixelToContent(newPosition, ActualPage, gAnchoredLayers.ActualWidth, gAnchoredLayers.ActualHeight, out contentBounds);

        if (content != null)
          SearchTerm = content;
        else SearchTerm = String.Empty;
        RemoveWordHighlight();
        if (contentBounds != Rect.Empty)
          AddWordHighlight(contentBounds);

        ShowContextualMenu();
      }
    }

    private void cHighlights_MouseMove(object sender, MouseEventArgs e)
    {
      if (ActualNote.Annotation != null)
        StickyNoteButton_MouseMove(sender, e);

      if (!isHighlighting)
        return;

      Point newPosition = Mouse.GetPosition(gAnchoredLayers);
      newHighlight.Line.X2 = newPosition.X;
      newHighlight.Line.Y2 = newPosition.Y;
    }

    private void gFixedLayers_MouseDown(object sender, MouseButtonEventArgs e)
    {
      lastPosition = Mouse.GetPosition(sender as Grid);
    }

    private void bHighlight_Click(object sender, RoutedEventArgs e)
    {
      if (!bHighlight.IsChecked.Value)
        bOffScreenVisualization.IsChecked = false;
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

    private void ShowContextualMenu()
    {
      if (!isSomething2Hide && !isHighlighting)
      {
        //close contextMenu at last position (in case user didn't choose a menu item)
        contextMenu.IsOpen = false;
        //open context menu at new position
        contextMenu.IsOpen = true;
        cm_deleteItem.Visibility = Visibility.Collapsed;
        contextMenu.Visibility = Visibility.Visible;        

        if (isSenderHighlight)
          cm_deleteItem.Visibility = Visibility.Visible;
      }
    }

    private void CMDelete_Click(object sender, RoutedEventArgs e)
    {
      cHighlights.Children.Remove(currentHighlight);
      foreach (Highlight line in ActualDocument[ActualPage].Highlights)
      {
        if (currentHighlight == line.Line)
        {
          ActualDocument[ActualPage].Highlights.Remove(line);
          break;
        }
      }      
    }

    private void CMSearch_Click(object sender, RoutedEventArgs e)
    {
      Search(SearchTerm, -1);
      SearchTerm = String.Empty;
      bSearch.IsChecked = true;
    }

    private void CMAnnotation_Click(object sender, RoutedEventArgs e)
    {
      //show keyboard and clean result
      tpKeyboard.Visibility = Visibility.Visible;
      tpKeyboard.ResultClear();

      Note newNote = new Note();
      newNote.Annotation = new StickyNote(lastPosition.X, lastPosition.Y);
      newNote.Annotation.BClose.Click += bStickyNoteClose_Click;
      newNote.Annotation.GNote.MouseMove += StickyNoteButton_MouseMove;
      newNote.Annotation.GNote.MouseDown += StickyNoteButton_MouseDown;
      newNote.Annotation.TextField.PreviewMouseDown += StickyNoteTextBox_PreviewMouseDown;
      newNote.Annotation.TextField.PreviewMouseMove += StickyNoteTextBox_PreviewMouseMove;
      //rotate sticky note
      //RotateTransform rotation = new RotateTransform(Device.Location.RotationAngle, newNote.annotation.Width * 0.5, newNote.annotation.Height * 0.5);
      //newNote.annotation.RenderTransform = rotation;


      newNote.Icon = new Image { Width = (int)iDocument.Width / 30, Height = (int)iDocument.Width / 25 };
      string strUri2 = (Environment.CurrentDirectory + "\\Images\\ICON.png");
      newNote.Icon.Source = new BitmapImage(new Uri(strUri2));
      newNote.Icon.Margin = new Thickness(lastPosition.X, lastPosition.Y - newNote.Icon.Height, 0, 0);
      newNote.Icon.MouseDown += Icon_MouseDown;

      cHighlights.Children.Add(newNote.Annotation);
      cHighlights.Children.Add(newNote.Icon);
      ActualDocument[ActualPage].Annotations.Add(newNote);

      //Update current note
      ActualNote = newNote;
    }
        
    private void bStickyNoteClose_Click(object sender, RoutedEventArgs e)
    {
      foreach (Note element in ActualDocument[ActualPage].Annotations)
      {
        if (element.Annotation.BClose == (Button)sender)
          ActualNote = element;
      }
      cHighlights.Children.Remove(ActualNote.Annotation);
      cHighlights.Children.Remove(ActualNote.Icon);
      ActualDocument[ActualPage].Annotations.Remove(ActualNote);
      ActualNote.Annotation = null;
      ActualNote.Icon = null;
      tpKeyboard.Visibility = Visibility.Hidden;
    }

    private void CMScribble_Click(object sender, RoutedEventArgs e)
    {
      Scribble newScribble = new Scribble();
      newScribble.Scribbling = new InkCanvas()
      {
        Background = Brushes.Beige,
        Width = (int)iDocument.Width / 7,
        Height = (int)iDocument.Width / 7,
      };

      newScribble.Scribbling.Margin = new Thickness(lastPosition.X, lastPosition.Y + 10, 0, 0);

      newScribble.Icon = new Image { Width = (int)iDocument.Width / 30, Height = (int)iDocument.Width / 25 };
      string strUri2 = (Environment.CurrentDirectory + "\\Images\\ICON.png");
      newScribble.Icon.Source = new BitmapImage(new Uri(strUri2));
      newScribble.Icon.Margin = new Thickness(lastPosition.X, lastPosition.Y - newScribble.Icon.Height, 0, 0);
      newScribble.Icon.MouseDown += ScribbleIcon_MouseDown;

      cHighlights.Children.Add(newScribble.Scribbling);
      cHighlights.Children.Add(newScribble.Icon);
      ActualDocument[ActualPage].Scribblings.Add(newScribble);
    }


    private void Icon_MouseDown(object sender, MouseButtonEventArgs e)
    {
      tpKeyboard.Visibility = Visibility.Hidden;
      foreach (Note element in ActualDocument[ActualPage].Annotations)
      {
        if (element.Icon == (Image)sender)
          ActualNote = element;
      }

      if (e.LeftButton == MouseButtonState.Pressed && e.RightButton == MouseButtonState.Released)
      {
        if (ActualNote.Annotation.Visibility == Visibility.Hidden)
          ActualNote.Annotation.Visibility = Visibility.Visible;
        else
          ActualNote.Annotation.Visibility = Visibility.Hidden;
      }
    }

    private Scribble ActualScribble;
    private void ScribbleIcon_MouseDown(object sender, MouseButtonEventArgs e)
    {
      foreach (Scribble element in ActualDocument[ActualPage].Scribblings)
      {
        if (element.Icon == (Image)sender)
          ActualScribble = element;
      }

      if (e.LeftButton == MouseButtonState.Pressed && e.RightButton == MouseButtonState.Released)
      {
        if (ActualScribble.Scribbling.Visibility == Visibility.Hidden)
          ActualScribble.Scribbling.Visibility = Visibility.Visible;
        else
          ActualScribble.Scribbling.Visibility = Visibility.Hidden;
      }
    }

    private void StickyNoteTextBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
      if (e.LeftButton == MouseButtonState.Pressed && e.RightButton == MouseButtonState.Released)
      {
        foreach (Note element in ActualDocument[ActualPage].Annotations)
        {
          if (element.Annotation.TextField == (TextBox)sender)
            ActualNote = element;
        }

        lastPosition = Mouse.GetPosition(gAnchoredLayers);
        tpKeyboard.Visibility = Visibility.Visible;
        tpKeyboard.ResultClear();
        tpKeyboard.CurrentText.Append(ActualNote.Annotation.TextField.Text);
      }
    }


    private void StickyNoteTextBox_PreviewMouseMove(object sender, MouseEventArgs e)
    {
      if (e.LeftButton == MouseButtonState.Pressed && e.RightButton == MouseButtonState.Released)
      {
        Point currentPosition = Mouse.GetPosition(gAnchoredLayers);
        Vector lineVector = new Vector(currentPosition.X - lastPosition.X,
          currentPosition.Y - lastPosition.Y);
        if (lineVector.Length > 10)
          ActualNote.Annotation.Margin = new Thickness(currentPosition.X, currentPosition.Y, 0, 0);
      }      
    }

    private void StickyNoteButton_MouseDown(object sender, MouseButtonEventArgs e)
    {
      foreach (Note element in ActualDocument[ActualPage].Annotations)
      {
        if (element.Annotation.GNote == (Grid)sender)
          ActualNote = element;
      }
    }

    static Size defaultNoteSize = new Size(20, 40);
    private void StickyNoteButton_MouseMove(object sender, MouseEventArgs e)
    {
      if (ActualNote.Annotation != null && ActualNote.Annotation.IsBResizeClicked)
      {        
        Point currentPosition = Mouse.GetPosition(gAnchoredLayers);
        Vector lineVector = new Vector(currentPosition.X - lastPosition.X,
          currentPosition.Y - lastPosition.Y);
        if (lineVector.Length > 5)
        {
            Point noteSize = new Point(currentPosition.X - ActualNote.Annotation.Margin.Left, currentPosition.Y - ActualNote.Annotation.Margin.Top);
            if (noteSize.X >= defaultNoteSize.Width)
              ActualNote.Annotation.GNote.Width = noteSize.X;
            if (noteSize.Y >= defaultNoteSize.Height)
              ActualNote.Annotation.GNote.Height = noteSize.Y;
        }
          tpKeyboard.Visibility = Visibility.Hidden;
      }       
    }

    private void bSearch_Click(object sender, RoutedEventArgs e)
    {
      if (bSearch.IsChecked.Value)
      {
        tpKeyboard.Visibility = Visibility.Visible;
        tpKeyboard.ResultClear();
      }
      else
      {
        bOffScreenVisualization.IsChecked = false;
        tpKeyboard.Visibility = Visibility.Hidden;
        ClearSearch();
      }
    }

    private void bCopyAndLock_Click(object sender, RoutedEventArgs e)
    {
      if (bCopyAndLock.IsChecked.Value)
        Core.Registration.Pause();
      else
        Core.Registration.Continue();
    }


    public void tpKeyboard_EnterKeyPressed(System.Object sender, EventArgs args)
    {
      if (bSearch.IsChecked.Value)
      {
        tpKeyboard.Visibility = Visibility.Hidden;
        Search(tpKeyboard.CurrentTextLine.ToString(), -1);
      }
      else if (bHighlight.IsChecked.Value && ActualNote.Annotation != null)
        ActualNote.Annotation.TextField.Text = tpKeyboard.CurrentText.ToString();
    }

    public void tpKeyboard_AlphaNumericKeyPressed(System.Object sender, EventArgs args)
    {
      if (bHighlight.IsChecked.Value && ActualNote.Annotation != null && !bSearch.IsChecked.Value)
        ActualNote.Annotation.TextField.Text = tpKeyboard.CurrentText.ToString();
    }

    //static public void SerializeToXML(List<ContentLocation> locations, string path)
    //{
    //  XmlSerializer serializer = new XmlSerializer(typeof(List<ContentLocation>));
    //  TextWriter textWriter = new StreamWriter(@path);
    //  serializer.Serialize(textWriter, locations);
    //  textWriter.Close();
    //}

    //static List<ContentLocation> DeserializeFromXML(string path)
    //{
    //  XmlSerializer deserializer = new XmlSerializer(typeof(List<ContentLocation>));
    //  TextReader textReader = new StreamReader(@path);
    //  List<ContentLocation> locations;
    //  locations = (List<ContentLocation>)deserializer.Deserialize(textReader);
    //  textReader.Close();

    //  return locations;
    //}
  }
}