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
using System.Windows.Ink;

namespace UofM.HCI.tPab.App.ActiveReader
{

  public enum ActiveReadingTool { None, Highlighter, Pen, Eraser };

  /// <summary>
  /// Interaction logic for ActiveReaderApp.xaml
  /// </summary>
  public partial class ActiveReaderApp : UserControl, ITPadApp, INotifyPropertyChanged
  {
    public event EventHandler Closed;

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

    private ActiveReadingTool currentTool = ActiveReadingTool.None;
    public ActiveReadingTool CurrentTool
    {
      get { return currentTool; }
      set
      {
        currentTool = value;
        OnPropertyChanged("CurrentTool");
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

      if (e.PropertyName == "CurrentTool")
        ArrangeLayersAccordingToTool();
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

      inkCScribble.DefaultDrawingAttributes.Height = 3 / Container.HeightFactor;
      inkCScribble.DefaultDrawingAttributes.Width = 3 / Container.WidthFactor;

      CurrentTool = ActiveReadingTool.None;
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
        LoadDocument(e.NewLocation);
      }
      else
      {
        //Change of document
        if (ActualDocument.ID != e.NewLocation.DocumentID)
        {
          //1- Saves current layers to disk
          PdfHelper.SaveLayersToDisk(ActualDocument, Core.Device.ID);

          //2- Loads the layers (for current page)
          LoadDocument(e.NewLocation);
        }
        // Change of page
        else if (ActualPage != e.NewLocation.PageIndex)
        {
          //1- Saves current layers to disk
          PdfHelper.SaveLayersToDisk(ActualDocument, Core.Device.ID);

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
      if (newLocation.DocumentID == -1)
        throw new Exception("Document cannot be null");
      if (!DbDocuments.ContainsKey(newLocation.DocumentID))
        throw new Exception("Unknown document");

      //1- Loads the layers should they exist in disk
      ActualDocument = DbDocuments[newLocation.DocumentID];
      PdfHelper = new PDFContentHelper(ActualDocument.FileName);
      PdfHelper.LoadLayersFromDisk(ActualDocument, Core.Device.ID);

      //3- Load layers for current page
      ActualPage = newLocation.PageIndex;
      LoadLayersToPage(ActualDocument, ActualPage);
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
          //var scribblings = cHighlights.Children.OfType<InkCanvas>().ToList();
          //foreach (InkCanvas scribble in scribblings)
          //  cHighlights.Children.Remove(scribble);

          //Unload existing scribblings
          inkCScribble.Strokes.Clear();

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
            cHighlights.Children.Add(linkHighlight.Line);
          }

          //Loads notes for this page
          foreach (Note element in document[pageIndex].Annotations)
          {
            Note note = (Note)element;
            cHighlights.Children.Add(note.Annotation);
            cHighlights.Children.Add(note.Icon);
          }

          //Loads scribbles for this page
          //foreach (Scribble element in document[pageIndex].Scribblings)
          //{
          //  Scribble note = (Scribble)element;
          //  cHighlights.Children.Add(note.Scribbling);
          //  cHighlights.Children.Add(note.Icon);
          //}

          //Loads scribbles for this page
          foreach (ScribbleCollection element in document[pageIndex].ScribblingCollections)
          {
            ScribbleCollection note = (ScribbleCollection)element;
            inkCScribble.Strokes.Add(note.ScribblingCollection);
          }
        });
    }

    private void ArrangeLayersAccordingToTool()
    {
      switch (currentTool)
      {
        case ActiveReadingTool.None:
        case ActiveReadingTool.Highlighter:
          inkCScribble.SetValue(Canvas.ZIndexProperty, 0);
          cHighlights.SetValue(Canvas.ZIndexProperty, 100);
          break;
        case ActiveReadingTool.Eraser:
        case ActiveReadingTool.Pen:
          cHighlights.SetValue(Canvas.ZIndexProperty, 0);
          inkCScribble.SetValue(Canvas.ZIndexProperty, 100);
          break;
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
        if (sender == rHighlights)
        {
          if (CurrentTool != ActiveReadingTool.Highlighter)
            return;

          isHighlighting = true;
          lastPosition = GetMousePositionInDocument();

          newHighlight = new Highlight();
          newHighlight.Line = new Line { Stroke = Brushes.YellowGreen, Opacity = 0.5, StrokeThickness = 18 / Container.HeightFactor };
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

        }
        else if (sender is Line)
        {
          Line line = (Line)sender;
          if (line.Tag != null)
          {
            isHighlighting = false; //to avoid highlighting in Figure-Mode
            ShowFigure((line.Tag as Figure));
          }
          else
          {
            isSenderHighlight = true;
            currentHighlight = line;
          }
        }
        else 
          isSenderHighlight = false;
      }
    }

    private void ShowFigure(Figure figure)
    {
      System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(ActualDocument.Pages[figure.PageIndex].FileName);

      BitmapSource source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty,
        System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
      CroppedImage = new CroppedBitmap(source, figure.FigureRect);

      figureViewer.Visibility = Visibility.Visible;
    }

    private float minlength_Highlight = (float)0.2; //cms
    private void cHighlights_MouseUp(object sender, MouseButtonEventArgs e)
    {
      Point newPosition = GetMousePositionInDocument();
      if (CurrentTool == ActiveReadingTool.Highlighter)
      {
        if (!isHighlighting)
          return;

        isHighlighting = false;
        newHighlight.Line.X2 = newPosition.X;
        newHighlight.Line.Y2 = newPosition.Y;

        Vector lineVector = new Vector(newHighlight.Line.X2 - newHighlight.Line.X1, newHighlight.Line.Y2 - newHighlight.Line.Y1);
        if (lineVector.Length > minlength_Highlight)
          ActualDocument[ActualPage].Highlights.Add(newHighlight);
        else
          cHighlights.Children.Remove(newHighlight.Line);
      }
      else //It was just a click to bring up the contextual menu
      {
        Rect contentBounds = Rect.Empty;
        String content = PdfHelper.PixelToContent(newPosition, ActualPage, Core.Profile.DocumentSize.Width, Core.Profile.DocumentSize.Height, out contentBounds);

        if (content != null)
          SearchTerm = content;
        else
          SearchTerm = String.Empty;
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

      if (CurrentTool != ActiveReadingTool.Highlighter)
        return;

      if (!isHighlighting)
        return;

      Point newPosition = GetMousePositionInDocument();
      newHighlight.Line.X2 = newPosition.X;
      newHighlight.Line.Y2 = newPosition.Y;
    }

    private Stroke currentStroke;
    private float defaultClusterStrokeDistanceCm = 5;
    private void inkCScribble_MouseUp(object sender, MouseButtonEventArgs e)
    {
      if (inkCScribble.Strokes.Count > 0)
        currentStroke = inkCScribble.Strokes[inkCScribble.Strokes.Count - 1];

      if (CurrentTool == ActiveReadingTool.Pen) //add strokes to Document (when strokes are close to each other cluster them in one strokeCollection)
      {
        foreach (ScribbleCollection scribbleCollection in ActualDocument[ActualPage].ScribblingCollections)
        {
          if (Distance(currentStroke.GetBounds().TopLeft, new System.Windows.Point(scribbleCollection.X, scribbleCollection.Y)) < defaultClusterStrokeDistanceCm ||
            Distance(currentStroke.GetBounds().BottomRight, new System.Windows.Point(scribbleCollection.X, scribbleCollection.Y)) < defaultClusterStrokeDistanceCm)
          {
            if (!scribbleCollection.ScribblingCollection.Contains(currentStroke))
              scribbleCollection.ScribblingCollection.Add(currentStroke);
            return;
          }
        }
        //if stroke is not close to another one, create new collection
        ScribbleCollection newCollection = new ScribbleCollection();
        newCollection.ScribblingCollection = new StrokeCollection();
        newCollection.ScribblingCollection.Add(currentStroke);
        ActualDocument[ActualPage].ScribblingCollections.Add(newCollection);
      }
      else if (CurrentTool == ActiveReadingTool.Eraser)
        inkCScribble.Strokes.Remove(currentStroke); //remove red eraser stroke
    }

    private float Distance(System.Windows.Point point1, System.Windows.Point point2)
    {
      return (float)Math.Sqrt((point1.X - point2.X) * (point1.X - point2.X) +
        (point1.Y - point2.Y) * (point1.Y - point2.Y));
    }

    private void inkCScribble_MouseMove(object sender, MouseEventArgs e)
    {
      if (e.LeftButton == MouseButtonState.Pressed && e.RightButton == MouseButtonState.Released &&
        CurrentTool == ActiveReadingTool.Eraser)
      {
        //delete strokes when crossing eraser
        Point currentMousePosition = GetMousePositionInDocument();
        foreach (ScribbleCollection collection in ActualDocument[ActualPage].ScribblingCollections)
        {
          foreach (Stroke stroke in collection.ScribblingCollection)
          {
            if (stroke.HitTest(currentMousePosition))
            {
              inkCScribble.Strokes.Remove(stroke);
              if (collection.ScribblingCollection.Count > 1)
                collection.ScribblingCollection.Remove(stroke);
              else
                ActualDocument[ActualPage].ScribblingCollections.Remove(collection);
              return;
            }
          }
        }

        var line = cHighlights.InputHitTest(currentMousePosition);
        if (line == null || !(line is Line))
          return;

        Highlight highlight = (Highlight)ActualDocument[ActualPage].Highlights.SingleOrDefault(tmp => (tmp as Highlight).Line == line);
        if (highlight == null)
          return;

        cHighlights.Children.Remove(highlight.Line);
        ActualDocument[ActualPage].Highlights.Remove(highlight);
      }
    }

    private void gFixedLayers_MouseDown(object sender, MouseButtonEventArgs e)
    {
      lastPosition = Mouse.GetPosition(gFixedLayers);
    }

    private void bLayers_Click(object sender, RoutedEventArgs e)
    {
      CurrentTool = ActiveReadingTool.None;
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
      newNote.Annotation.GNote.MouseUp += StickyNoteButton_MouseUp;
      newNote.Annotation.TextField.PreviewMouseDown += StickyNoteTextBox_PreviewMouseDown;
      newNote.Annotation.TextField.PreviewMouseMove += StickyNoteTextBox_PreviewMouseMove;

      newNote.Annotation.WidthFactor = Container.WidthFactor;
      newNote.Annotation.HeightFactor = Container.HeightFactor;
      newNote.Annotation.Width = 150;
      newNote.Annotation.Height = 150;

      //rotate sticky note
      //RotateTransform rotation = new RotateTransform(Device.Location.RotationAngle, newNote.annotation.Width * 0.5, newNote.annotation.Height * 0.5);
      //newNote.annotation.RenderTransform = rotation;

      newNote.Icon = new Image { Width = 1, Height = 0.8 };
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

    private void StickyNoteTextBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
      if (e.LeftButton == MouseButtonState.Pressed && e.RightButton == MouseButtonState.Released)
      {
        foreach (Note element in ActualDocument[ActualPage].Annotations)
        {
          if (element.Annotation.TextField == (TextBox)sender)
            ActualNote = element;
        }

        ActualNote.Annotation.IsBResizeClicked = false;
        ActualNote.Annotation.IsNoteMoving = true;

        lastPosition = GetMousePositionInDocument();

        tpKeyboard.ResultClear();
        tpKeyboard.CurrentText.Append(ActualNote.Annotation.TextField.Text);
        tpKeyboard.Visibility = Visibility.Visible;
      }
    }

    private void StickyNoteTextBox_PreviewMouseMove(object sender, MouseEventArgs e)
    {
      if (e.LeftButton == MouseButtonState.Pressed && e.RightButton == MouseButtonState.Released && ActualNote.Annotation.IsNoteMoving)
      {
        Point currentPosition = GetMousePositionInDocument();
        Vector lineVector = new Vector(currentPosition.X - lastPosition.X,
          currentPosition.Y - lastPosition.Y);

        if (lineVector.Length > 1)
        {
          ActualNote.Annotation.Margin = new Thickness(currentPosition.X, currentPosition.Y, 0, 0);
          tpKeyboard.Visibility = Visibility.Hidden;
        }
      }
    }

    private void StickyNoteButton_MouseDown(object sender, MouseButtonEventArgs e)
    {
      if (e.LeftButton == MouseButtonState.Pressed && e.RightButton == MouseButtonState.Released)
      {
        ActualNote.Annotation.IsBResizeClicked = true;
        ActualNote.Annotation.IsNoteMoving = false;

        foreach (Note element in ActualDocument[ActualPage].Annotations)
        {
          if (element.Annotation.GNote == (Grid)sender)
            ActualNote = element;
        }
      }
    }

    static Size defaultNoteSize = new Size(1, 1);
    private void StickyNoteButton_MouseMove(object sender, MouseEventArgs e)
    {
      if (e.LeftButton == MouseButtonState.Pressed && e.RightButton == MouseButtonState.Released && ActualNote.Annotation != null && ActualNote.Annotation.IsBResizeClicked)
      {
        Point currentPosition = GetMousePositionInDocument();
        Vector lineVector = new Vector(currentPosition.X - lastPosition.X,
          currentPosition.Y - lastPosition.Y);

        if (lineVector.Length > 1)
        {
          Point noteSize = new Point(currentPosition.X - ActualNote.Annotation.Margin.Left, currentPosition.Y - ActualNote.Annotation.Margin.Top);
          if (noteSize.X >= defaultNoteSize.Width)
            ActualNote.Annotation.Width = noteSize.X * Container.WidthFactor;
          if (noteSize.Y >= defaultNoteSize.Height)
            ActualNote.Annotation.Height = noteSize.Y * Container.HeightFactor;
        }
        tpKeyboard.Visibility = Visibility.Hidden;
      }
    }

    private void StickyNoteButton_MouseUp(object sender, MouseEventArgs e)
    {
      ActualNote.Annotation.IsBResizeClicked = false;
    }

    private void bHighlightTool_Click(object sender, RoutedEventArgs e)
    {
    }

    private void bScribbleTool_Click(object sender, RoutedEventArgs e)
    {
      inkCScribble.DefaultDrawingAttributes.Color = Color.FromRgb(0, 0, 0);
    }

    private void bEraseTool_Click(object sender, RoutedEventArgs e)
    {
      inkCScribble.DefaultDrawingAttributes.Color = Color.FromRgb(255, 0, 0);
    }

    private void bCopyAndLock_Click(object sender, RoutedEventArgs e)
    {
      if (bCopyAndLock.IsChecked.Value)
        Core.Registration.Pause();
      else
        Core.Registration.Continue();
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
        tpKeyboard.Visibility = Visibility.Hidden;
        ClearSearch();
      }
    }

    private void Search(String word, int page)
    {
      ClearSearch();

      List<ContentLocation> pageSearch = PdfHelper.ContentToPixel(word, page, Core.Profile.DocumentSize.Width, Core.Profile.DocumentSize.Height);

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

    private void ClearSearch()
    {
      foreach (ActiveReaderPage documentPage in ActualDocument.Pages)
        documentPage.SearchResults.Clear();
      cSearchResults.Children.Clear();
    }

    public void tpKeyboard_EnterKeyPressed(System.Object sender, EventArgs args)
    {
      if (bSearch.IsChecked.Value)
      {
        tpKeyboard.Visibility = Visibility.Hidden;
        Search(tpKeyboard.CurrentTextLine.ToString(), -1);
      }
      else if (bLayers.IsChecked.Value && ActualNote.Annotation != null)
        ActualNote.Annotation.TextField.Text = tpKeyboard.CurrentText.ToString();
    }

    public void tpKeyboard_AlphaNumericKeyPressed(System.Object sender, EventArgs args)
    {
      if (bLayers.IsChecked.Value && ActualNote.Annotation != null && !bSearch.IsChecked.Value)
        ActualNote.Annotation.TextField.Text = tpKeyboard.CurrentText.ToString();
    }

    /// <summary>
    /// Gets the click position in the underlaying document in cms
    /// </summary>
    private Point GetMousePositionInDocument()
    {
      Point mouseDocPosition = Mouse.GetPosition(gAnchoredLayers);
      mouseDocPosition.X = mouseDocPosition.X / Container.WidthFactor;
      mouseDocPosition.Y = mouseDocPosition.Y / Container.HeightFactor;
      return mouseDocPosition;
    }

    public void Close()
    {
      if (Closed != null)
        Closed(this, EventArgs.Empty);
    }

    private void bExit_Click(object sender, RoutedEventArgs e)
    {
      Close();
    }

    public event PropertyChangedEventHandler PropertyChanged;
    public void OnPropertyChanged(String name)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(name));

      if (name == "ActualPage" || name == "ActualDocument")
        OnPropertyChanged("ActualPageObject");
    }

  }
}