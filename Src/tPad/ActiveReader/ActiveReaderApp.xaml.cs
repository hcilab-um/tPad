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
using UofM.HCI.tPad.Network;

namespace UofM.HCI.tPad.App.ActiveReader
{

  public enum ActiveReadingTool { None, Highlighter, Pen, Eraser };

  /// <summary>
  /// Interaction logic for ActiveReaderApp.xaml
  /// </summary>
  public partial class ActiveReaderApp : UserControl, ITPadApp, INotifyPropertyChanged
  {
    public event BoolEventHandler IsTopApp;
    public event RequestUserFocus RequestFocus;
    public event EventHandler Closed;
    public event RequestAction RequestAction;

    public Dictionary<int, ActiveReaderDocument> DbDocuments { get; set; }

    public Guid AppUUID { get; private set; }
    public TPadCore Core { get; set; }
    public ITPadAppContainer Container { get; set; }
    public ITPadAppController Controller { get; set; }
    public Dictionary<String, System.Object> Context { get { return null; } }

    private PDFContentHelper PdfHelper { get; set; }
    private SynchHelper Synch { get; set; }
    private Guid strokeObjectPropertyID = new Guid("00000000-0000-0000-0000-000000000001");
    private Guid strokeCollectionPropertyID = new Guid("00000000-0000-0000-0000-000000000002");

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

    private double zoomLevel = 1;
    public double ZoomLevel
    {
      get { return zoomLevel; }
      set
      {
        zoomLevel = value;
        OnPropertyChanged("ZoomLevel");
      }
    }

    public bool RedoAvailable
    {
      get { return redoStack.Count == 0 ? false : true; }
    }

    private Stack<ToolObjectPair> undoStack = new Stack<ToolObjectPair>();
    private Stack<ToolObjectPair> redoStack = new Stack<ToolObjectPair>();

    public ActiveReaderApp(TPadCore core, ITPadAppContainer container, ITPadAppController controller, Guid appUUID)
    {
      Core = core;
      AppUUID = appUUID;

      ActualPage = -1;
      ActualDocument = null;

      Container = container;
      Controller = controller;

      DbDocuments = new Dictionary<int, ActiveReaderDocument>();

      PropertyChanged += new PropertyChangedEventHandler(ActiveReaderApp_PropertyChanged);
      InitializeComponent();
    }

    public void Activate(Dictionary<string, System.Object> init) { }

    void ActiveReaderApp_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == "ActualPage" || e.PropertyName == "ActualDocument")
      {
        OnPropertyChanged("ActualPageObject");
      }

      if (e.PropertyName == "CurrentTool")
        ArrangeLayersAccordingToTool();
    }

    private void arApp_Loaded(object sender, RoutedEventArgs e)
    {
      Core.Device.RegistrationChanged += new RegistrationChangedEventHandler(Device_RegistrationChanged);
      Core.Device.FlippingChanged += new FlippingChangedEventHandler(Device_FlippingChanged);
      Core.Device.DeviceShaked += new EventHandler(Device_DeviceShaked);

      Core.Device.StackingChanged += new StackingChangedEventHandler(Device_StackingChanged);
      Core.Device.StackingTouchEvent += new StackingTouchEventEventHandler(Device_StackingTouchEvent);

      Synch = new SynchHelper(Core, Dispatcher);
      Synch.SynchContent += new SynchContentEventHandler(Synch_SynchContent);

      BindingOperations.SetBinding(cm_searchItem, MenuItem.HeaderProperty, new Binding("SearchTerm")
      {
        Source = this,
        Converter = new UofM.HCI.tPad.App.ActiveReader.Converters.StringToContextMenuLabelConverter(),
        ConverterParameter = "Search \"{0}\""
      });

      BindingOperations.SetBinding(cm_searchItem, MenuItem.VisibilityProperty, new Binding("SearchTerm")
      {
        Source = this,
        Converter = new UofM.HCI.tPad.App.ActiveReader.Converters.ContextMenuVisibilityConverter(),
      });

      //necessary to set binding of context menu
      NameScope.SetNameScope(contextMenu, NameScope.GetNameScope(this));

      inkCScribble.DefaultDrawingAttributes.Height = 3 / Core.Profile.PixelsPerCm.Height;
      inkCScribble.DefaultDrawingAttributes.Width = 3 / Core.Profile.PixelsPerCm.Width;

      CurrentTool = ActiveReadingTool.None;
      Device_RegistrationChanged(this, new RegistrationEventArgs() { NewLocation = Core.Device.Location });
    }

    void Device_DeviceShaked(object sender, EventArgs e)
    {
      if (figureViewer.Visibility == System.Windows.Visibility.Visible)
      {
        figureViewer.Visibility = System.Windows.Visibility.Collapsed;
      }
      else if (CurrentTool == ActiveReadingTool.None)
      {
        bLayers.IsChecked = false;
      }
      else
      {
        ProcessUndoRequest();
      }
    }

    void Device_FlippingChanged(object sender, FlippingEventArgs e)
    {
    }

    void Device_RegistrationChanged(object sender, RegistrationEventArgs e)
    {
      DateTime start = DateTime.Now;
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
        if (!ActualDocument.Equals(e.NewLocation.Document))
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
          undoStack.Clear();
          redoStack.Clear();
        }
      }

      Dispatcher.Invoke(DispatcherPriority.Render,
        (Action)delegate()
        {
          Point locationPx = new Point(
              e.NewLocation.LocationCm.X * Core.Profile.PixelsPerCm.Width,
              e.NewLocation.LocationCm.Y * Core.Profile.PixelsPerCm.Height);

          trCanvas.Angle = e.NewLocation.RotationAngle * -1;
          trCanvas.CenterX = locationPx.X;
          trCanvas.CenterY = locationPx.Y;

          ttCanvas.X = locationPx.X * -1 + ActualWidth / 2;
          ttCanvas.Y = locationPx.Y * -1 + ActualHeight / 2;

          //Rotation based functionalities
          ProcessContrastUpdate(e);
          ProcessZoomUpdate(e);
          ProcessAreaTriggers(e.NewLocation, locationPx);
        });
    }

    private bool isPlaying = false;
    private void ProcessAreaTriggers(TPadLocation tPadLocation, Point locationPx)
    {
      if (tPadLocation.PageIndex != 2)
        return;

      Rect deviceCenterArea = new Rect(new Point(-100, -100), new Size(200, 200));
      deviceCenterArea.Offset(locationPx.X, locationPx.Y);

      Point areaTrigger = new Point(950, 1200);

      if (deviceCenterArea.Contains(areaTrigger))
      {
        //if (isPlaying)
        //  return;

        System.Windows.Interop.HwndSource hwndSource = PresentationSource.FromVisual(this) as System.Windows.Interop.HwndSource;
        System.Windows.Interop.HwndTarget hwndTarget = hwndSource.CompositionTarget;
        hwndTarget.RenderMode = System.Windows.Interop.RenderMode.SoftwareOnly;

        //Console.WriteLine("meVideo.Play();");
        meVideo.Visibility = System.Windows.Visibility.Visible;
        meVideo.Play();
        isPlaying = true;
      }
      else if (isPlaying)
      {
        //Console.WriteLine("meVideo.Pause();");
        //meVideo.Visibility = System.Windows.Visibility.Collapsed;
        //meVideo.Pause();
        //isPlaying = false;
      }
    }

    private void LoadDocument(TPadLocation newLocation)
    {
      if (newLocation.Document == null)
        throw new Exception("Document cannot be null");

      if (!DbDocuments.ContainsKey(newLocation.Document.ID))
      {
        Dispatcher.Invoke(DispatcherPriority.Render,
        (Action)delegate()
        {
          DocumentLoader loader = new DocumentLoader();
          ActiveReaderDocument lookup = loader.LoadDocument(newLocation.Document);
          DbDocuments.Add(lookup.ID, lookup);
        });
      }

      //1- Loads the layers should they exist in disk
      ActualDocument = DbDocuments[newLocation.Document.ID];
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
          var notes = cHighlights.Children.OfType<StickyNote>().ToList();
          foreach (StickyNote note in notes)
            cHighlights.Children.Remove(note);

          //Unload existing icons
          var icons = cHighlights.Children.OfType<Image>().ToList();
          foreach (Image icon in icons)
            cHighlights.Children.Remove(icon);

          //Unload existing scribblings
          inkCScribble.Strokes.Clear();

          //Unload existing search results
          cSearchResults.Children.Clear();

          //Loads other highlights for this page
          foreach (Highlight element in document[pageIndex].Highlights)
            AddHighlight(element.Line, cHighlights);

          //Loads search results for this page
          foreach (Highlight element in document[pageIndex].SearchResults)
            AddHighlight(element.Line, cSearchResults);

          //Loads figure links for this page
          foreach (Highlight element in document[pageIndex].FigureLinks)
            AddHighlight(element.Line, cHighlights);

          //Loads notes for this page
          foreach (Note element in document[pageIndex].Annotations)
          {
            Note note = (Note)element;
            cHighlights.Children.Add(note.Annotation);
            cHighlights.Children.Add(note.Icon);
          }

          //Loads scribbles for this page
          foreach (ScribbleCollection element in document[pageIndex].ScribblingCollections)
          {
            ScribbleCollection note = (ScribbleCollection)element;
            inkCScribble.Strokes.Add(note.Scribbles);
          }
        });
    }

    private void AddHighlight(Line highlight, Canvas canvas)
    {
      highlight.MouseDown += cHighlights_MouseDown;
      highlight.MouseMove += cHighlights_MouseMove;
      highlight.MouseUp += cHighlights_MouseUp;
      canvas.Children.Add(highlight);
    }

    private void RemoveHighlight(Line highlight, Canvas canvas)
    {
      highlight.MouseDown -= cHighlights_MouseDown;
      highlight.MouseMove -= cHighlights_MouseMove;
      highlight.MouseUp -= cHighlights_MouseUp;
      canvas.Children.Remove(highlight);
    }

    private void AddNote(Note note)
    {
      note.Annotation.getBClose().Click += bStickyNoteClose_Click;
      note.Annotation.getGNote().MouseMove += StickyNoteButton_MouseMove;
      note.Annotation.getGNote().MouseDown += StickyNoteButton_MouseDown;
      note.Annotation.getGNote().MouseUp += StickyNoteButton_MouseUp;
      note.Annotation.getTextField().PreviewMouseDown += StickyNoteTextBox_PreviewMouseDown;
      note.Annotation.getTextField().PreviewMouseMove += StickyNoteTextBox_PreviewMouseMove;
      note.Icon.MouseDown += Icon_MouseDown;
      cHighlights.Children.Add(note.Annotation);
      cHighlights.Children.Add(note.Icon);
    }

    private void RemoveNote(Note note)
    {
      note.Annotation.getBClose().Click -= bStickyNoteClose_Click;
      note.Annotation.getGNote().MouseMove -= StickyNoteButton_MouseMove;
      note.Annotation.getGNote().MouseDown -= StickyNoteButton_MouseDown;
      note.Annotation.getGNote().MouseUp -= StickyNoteButton_MouseUp;
      note.Annotation.getTextField().PreviewMouseDown -= StickyNoteTextBox_PreviewMouseDown;
      note.Annotation.getTextField().PreviewMouseMove -= StickyNoteTextBox_PreviewMouseMove;
      note.Icon.MouseDown -= Icon_MouseDown;
      cHighlights.Children.Remove(note.Annotation);
      cHighlights.Children.Remove(note.Icon);
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
    //private Line currentHighlight;
    private bool isSomething2Hide = false;
    //private bool isSenderHighlight = false;
    private void cHighlights_MouseDown(object sender, MouseButtonEventArgs e)
    {
      if (Core.Device.State != StackingState.NotStacked)
        return;

      if (e.LeftButton == MouseButtonState.Pressed && e.RightButton == MouseButtonState.Released)
      {
        lastPosition = GetMousePositionInDocument();
        if (sender == rHighlights)
        {
          if (CurrentTool != ActiveReadingTool.Highlighter)
          {
            //isSenderHighlight = false;
            //Hide pop-up stick notes
            isSomething2Hide = false;
            foreach (Note element in ActualDocument[ActualPage].Annotations)
            {
              if (element.Annotation.Visibility == Visibility.Visible)
              {
                isSomething2Hide = true;
                element.Annotation.Visibility = Visibility.Collapsed;
              }
            }
            return;
          }
          else if (CurrentTool == ActiveReadingTool.Highlighter)
          {
            isHighlighting = true;
            newHighlight = new Highlight();
            newHighlight.Line = new Line { Stroke = Brushes.YellowGreen, Opacity = 0.5, StrokeThickness = 18 / Core.Profile.PixelsPerCm.Height };

            newHighlight.Line.X1 = lastPosition.X;
            newHighlight.Line.Y1 = lastPosition.Y;
            newHighlight.Line.X2 = lastPosition.X;
            newHighlight.Line.Y2 = lastPosition.Y;
            AddHighlight(newHighlight.Line, cHighlights);
          }
        }
        else if (sender is Line)
        {
          Line line = (Line)sender;
          if (line.Tag != null && line.Tag is Figure)
          {
            isHighlighting = false; //to avoid highlighting in Figure-Mode
            ShowFigure((line.Tag as Figure));
          }
          //else
          //{
          //  isSenderHighlight = true;
          //  currentHighlight = line;
          //}
        }
      }
    }

    private float minlength_Highlight = (float)0.2; //cms
    private bool isSearchHighlightActive = false;
    private void cHighlights_MouseUp(object sender, MouseButtonEventArgs e)
    {
      if (Core.Device.State != StackingState.NotStacked)
        return;

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
        {
          ActualDocument[ActualPage].Highlights.Add(newHighlight);

          //creates a point for undo for this action
          PushToUndoStack(ActiveReadingTool.Highlighter, newHighlight);
        }
        else
          cHighlights.Children.Remove(newHighlight.Line);
      }
      else //It was just a click to bring up the contextual menu
      {
        RemoveWordHighlight();
        if (contextMenu.Visibility == Visibility.Visible || tpKeyboard.Visibility == Visibility.Visible)
        {
          //SearchTerm = String.Empty;
          isSearchHighlightActive = false;
          contextMenu.Visibility = System.Windows.Visibility.Collapsed;
          tpKeyboard.Visibility = Visibility.Collapsed;
          return;
        }

        Rect contentBounds = Rect.Empty;
        String content = PdfHelper.PixelToContent(newPosition, ActualPage, ActualDocument.DocumentSize.Width, ActualDocument.DocumentSize.Height, out contentBounds);
        if (content != null && !isSearchHighlightActive && !isFigureViewerVisible)
        {
          SearchTerm = content;
          AddWordHighlight(contentBounds, content);
          ShowContextualMenu();
          isSearchHighlightActive = true;
        }
        else if (content == null && !isSearchHighlightActive)
          ShowContextualMenu();
        else if (content == null || isSearchHighlightActive)
          isSearchHighlightActive = false;
      }

      isFigureViewerVisible = false;
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

    private bool isFigureViewerVisible = false;
    private void ShowFigure(Figure figure)
    {
      System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(ActualDocument.Pages[figure.PageIndex].FileName);

      BitmapSource source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty,
        System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
      CroppedImage = new CroppedBitmap(source, figure.FigureRect);

      contextMenu.Visibility = System.Windows.Visibility.Collapsed;
      figureViewer.Visibility = Visibility.Visible;
      isFigureViewerVisible = true;
    }

    private Stroke currentStroke;
    private float defaultClusterStrokeDistanceCm = 5;
    private void inkCScribble_MouseUp(object sender, MouseButtonEventArgs e)
    {
      if (inkCScribble.Strokes.Count > 0)
      {
        currentStroke = inkCScribble.Strokes[inkCScribble.Strokes.Count - 1];
        currentStroke.AddPropertyData(strokeObjectPropertyID, Guid.NewGuid().ToString());
      }

      if (CurrentTool == ActiveReadingTool.Pen) //add strokes to Document (when strokes are close to each other cluster them in one strokeCollection)
      {
        foreach (ScribbleCollection collection in ActualDocument[ActualPage].ScribblingCollections)
        {
          if (Distance(currentStroke.GetBounds().TopLeft, new System.Windows.Point(collection.X, collection.Y)) < defaultClusterStrokeDistanceCm ||
            Distance(currentStroke.GetBounds().BottomRight, new System.Windows.Point(collection.X, collection.Y)) < defaultClusterStrokeDistanceCm)
          {
            if (!collection.Scribbles.Contains(currentStroke))
            {
              collection.Scribbles.Add(currentStroke);
              currentStroke.AddPropertyData(strokeCollectionPropertyID, collection.ID.ToString());

              //creates a point for undo for this action
              PushToUndoStack(ActiveReadingTool.Pen, collection, currentStroke);
            }
            return;
          }
        }

        //if stroke is not close to another one, create new collection
        ScribbleCollection newCollection = new ScribbleCollection();
        newCollection.Scribbles = new StrokeCollection();
        newCollection.Scribbles.Add(currentStroke);
        currentStroke.AddPropertyData(strokeCollectionPropertyID, newCollection.ID.ToString());
        ActualDocument[ActualPage].ScribblingCollections.Add(newCollection);

        //creates a point for undo for this action
        PushToUndoStack(ActiveReadingTool.Pen, newCollection);
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
          foreach (Stroke stroke in collection.Scribbles)
          {
            if (stroke.HitTest(currentMousePosition))
            {
              inkCScribble.Strokes.Remove(stroke);
              if (collection.Scribbles.Count > 1)
              {
                collection.Scribbles.Remove(stroke);

                //creates a point for undo for this action
                PushToUndoStack(ActiveReadingTool.Eraser, collection, stroke);
              }
              else
              {
                ActualDocument[ActualPage].ScribblingCollections.Remove(collection);

                //creates a point for undo for this action
                PushToUndoStack(ActiveReadingTool.Eraser, collection);
              }
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

        //creates a point for undo for this action
        PushToUndoStack(ActiveReadingTool.Eraser, highlight);
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
      SearchTerm = String.Empty;

      if (wordHighlight == null)
        return;

      cHighlights.Children.Remove(wordHighlight);
    }

    private Line wordHighlight;
    private void AddWordHighlight(Rect wordBounds, String word)
    {
      wordHighlight = new Line() { Stroke = Brushes.Pink, Opacity = 0.5, StrokeThickness = wordBounds.Height };
      wordHighlight.X1 = wordBounds.Left;
      wordHighlight.Y1 = wordBounds.Top + wordBounds.Height / 2;
      wordHighlight.X2 = wordBounds.Right;
      wordHighlight.Y2 = wordBounds.Top + wordBounds.Height / 2;
      wordHighlight.Tag = word;
      AddHighlight(wordHighlight, cHighlights);
    }

    private void ShowContextualMenu()
    {
      if (!isSomething2Hide && !isHighlighting && !isSearchHighlightActive && !isFigureViewerVisible && contextMenu.Visibility != Visibility.Visible)
      {
        //close contextMenu at last position (in case user didn't choose a menu item)
        contextMenu.IsOpen = false;
        //open context menu at new position
        contextMenu.IsOpen = true;
        //cm_deleteItem.Visibility = Visibility.Collapsed;
        contextMenu.Visibility = Visibility.Visible;

        //if (isSenderHighlight)
        //  cm_deleteItem.Visibility = Visibility.Visible;
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
      tpKeyboard.ResultClear();
      tpKeyboard.Visibility = Visibility.Visible;

      Note newNote = new Note();
      newNote.Annotation = new StickyNote(lastPosition.X, lastPosition.Y);
      newNote.Annotation.Width = 150;
      newNote.Annotation.Height = 150;
      newNote.Annotation.WidthFactor = Core.Profile.PixelsPerCm.Width;
      newNote.Annotation.HeightFactor = Core.Profile.PixelsPerCm.Height;

      newNote.Icon = new Image { Width = 1, Height = 0.8 };
      string strUri2 = (Environment.CurrentDirectory + "\\Images\\ICON.png");
      newNote.Icon.Source = new BitmapImage(new Uri(strUri2));
      newNote.Icon.Margin = new Thickness(lastPosition.X, lastPosition.Y - newNote.Icon.Height, 0, 0);

      AddNote(newNote);
      ActualDocument[ActualPage].Annotations.Add(newNote);

      //Update current note
      ActualNote = newNote;
    }

    private void bStickyNoteClose_Click(object sender, RoutedEventArgs e)
    {
      foreach (Note element in ActualDocument[ActualPage].Annotations)
      {
        if (element.Annotation.getBClose() == (Button)sender)
          ActualNote = element;
      }
      cHighlights.Children.Remove(ActualNote.Annotation);
      cHighlights.Children.Remove(ActualNote.Icon);
      ActualDocument[ActualPage].Annotations.Remove(ActualNote);
      ActualNote.Annotation = null;
      ActualNote.Icon = null;
      tpKeyboard.Visibility = Visibility.Collapsed;
    }

    private void Icon_MouseDown(object sender, MouseButtonEventArgs e)
    {
      RemoveWordHighlight();
      contextMenu.Visibility = System.Windows.Visibility.Collapsed;
      tpKeyboard.Visibility = Visibility.Collapsed;
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
          if (element.Annotation.getTextField() == (TextBox)sender)
            ActualNote = element;
        }

        ActualNote.Annotation.IsBResizeClicked = false;
        ActualNote.Annotation.IsNoteMoving = true;

        lastPosition = GetMousePositionInDocument();

        tpKeyboard.ResultClear();
        tpKeyboard.CurrentText.Append(ActualNote.Annotation.getTextField().Text);
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
          tpKeyboard.Visibility = Visibility.Collapsed;
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
          if (element.Annotation.getGNote() == (Grid)sender)
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
            ActualNote.Annotation.Width = noteSize.X * Core.Profile.PixelsPerCm.Width;
          if (noteSize.Y >= defaultNoteSize.Height)
            ActualNote.Annotation.Height = noteSize.Y * Core.Profile.PixelsPerCm.Height;
        }
        tpKeyboard.Visibility = Visibility.Collapsed;
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
        tpKeyboard.Visibility = Visibility.Collapsed;
        ClearSearch();
      }
    }

    private void Search(String word, int page)
    {
      ClearSearch();

      List<ContentLocation> pageSearch = PdfHelper.ContentToPixel(word, page, ActualDocument.DocumentSize.Width, ActualDocument.DocumentSize.Height);

      foreach (ContentLocation content in pageSearch)
      {
        Highlight resultHL = new Highlight();
        resultHL.Line = new Line() { Stroke = Brushes.Blue, Opacity = 0.5, StrokeThickness = content.ContentBounds.Height };
        resultHL.Line.X1 = content.ContentBounds.Left;
        resultHL.Line.Y1 = content.ContentBounds.Top + content.ContentBounds.Height / 2;
        resultHL.Line.X2 = content.ContentBounds.Right;
        resultHL.Line.Y2 = content.ContentBounds.Top + content.ContentBounds.Height / 2;
        resultHL.Line.Tag = content.Content;
        if (content.PageIndex == ActualPage)
          AddHighlight(resultHL.Line, cSearchResults);
        ActualDocument[content.PageIndex].SearchResults.Add(resultHL);
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
        tpKeyboard.Visibility = Visibility.Collapsed;
        Search(tpKeyboard.CurrentTextLine.ToString(), -1);
      }
      else if (bLayers.IsChecked.Value && ActualNote.Annotation != null)
        ActualNote.Annotation.getTextField().Text = tpKeyboard.CurrentText.ToString();
    }

    public void tpKeyboard_AlphaNumericKeyPressed(System.Object sender, EventArgs args)
    {
      if (bLayers.IsChecked.Value && ActualNote.Annotation != null && !bSearch.IsChecked.Value)
        ActualNote.Annotation.getTextField().Text = tpKeyboard.CurrentText.ToString();
    }

    /// <summary>
    /// Gets the click position in the underlaying document in cms
    /// </summary>
    private Point GetMousePositionInDocument()
    {
      Point mouseDocPosition = Mouse.GetPosition(gAnchoredLayers);
      mouseDocPosition.X = mouseDocPosition.X / Core.Profile.PixelsPerCm.Width;
      mouseDocPosition.Y = mouseDocPosition.Y / Core.Profile.PixelsPerCm.Height;
      return mouseDocPosition;
    }

    public void DeActivate() { }

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
    }

    private double initialAngle = 0, initialOpacity = 1;
    private void bContrast_Click(object sender, RoutedEventArgs e)
    {
      if (!bContrast.IsChecked.Value)
        return;

      initialAngle = Core.Device.Location.RotationAngle;
      initialOpacity = gOuterWrapper.Opacity;
    }

    private void ProcessContrastUpdate(RegistrationEventArgs e)
    {
      if (!bContrast.IsChecked.Value)
        return;

      var lastAngle = initialAngle;
      if (lastAngle > 180)
        lastAngle = lastAngle - 360;

      var newAngle = e.NewLocation.RotationAngle;
      if (newAngle > 180)
        newAngle = newAngle - 360;

      var angle = newAngle - lastAngle;

      //Opacity moves between 0 and 1 
      // by design we think that the complete change from 0 to 1 should be accomplished in 90 degrees
      var change = angle / 90;
      var newOpacity = initialOpacity + change;
      if (newOpacity < 0)
        newOpacity = 0;
      if (newOpacity > 1)
        newOpacity = 1;

      gOuterWrapper.Opacity = newOpacity;
    }

    private double initialZoomLevel = 1;
    private void bZoom_Click(object sender, RoutedEventArgs e)
    {
      if (!bZoom.IsChecked.Value)
        return;

      initialAngle = Core.Device.Location.RotationAngle;
      initialZoomLevel = ZoomLevel;
    }

    private void ProcessZoomUpdate(RegistrationEventArgs e)
    {
      if (!bZoom.IsChecked.Value)
        return;

      var lastAngle = initialAngle;
      if (lastAngle > 180)
        lastAngle = lastAngle - 360;

      var newAngle = e.NewLocation.RotationAngle;
      if (newAngle > 180)
        newAngle = newAngle - 360;

      var angle = newAngle - lastAngle;

      //ZoomLevel moves between 1 and 5
      // by design we think that the complete change from 1 to 2 should be accomplished in 45 degrees
      var change = angle / 45;
      var newZoomLevel = initialZoomLevel + change;
      if (newZoomLevel < 1)
        newZoomLevel = 1;
      if (newZoomLevel > 5)
        newZoomLevel = 5;

      ZoomLevel = newZoomLevel;
    }

    private void bSettings_Click(object sender, RoutedEventArgs e)
    {
      if (bSettings.IsChecked.Value)
        return;

      bContrast.IsChecked = false;
      bZoom.IsChecked = false;
    }

    private void PushToUndoStack(ActiveReadingTool tool, System.Object parameter1, System.Object parameter2 = null)
    {
      List<System.Object> parameters = new List<System.Object>();
      if (parameter1 != null)
        parameters.Add(parameter1);
      if (parameter2 != null)
        parameters.Add(parameter2);

      PushToUndoStack(new ToolObjectPair() { Tool = tool, Parameters = parameters.ToArray() }, true);
    }

    private void PushToUndoStack(ToolObjectPair action, bool clearRedoStack)
    {
      undoStack.Push(action);
      if (clearRedoStack)
        redoStack.Clear();
      OnPropertyChanged("RedoAvailable");
    }

    private void PushToRedoStack(ToolObjectPair action)
    {
      redoStack.Push(action);
      OnPropertyChanged("RedoAvailable");
    }

    public void ProcessUndoRequest()
    {
      if (undoStack.Count == 0)
        return;

      ToolObjectPair actionToUndo = undoStack.Pop();
      switch (actionToUndo.Tool)
      {
        case ActiveReadingTool.Highlighter:
          {
            Highlight highlight = (Highlight)actionToUndo.Parameters[0];
            RemoveHighlight(highlight.Line, cHighlights);
            ActualDocument[ActualPage].Highlights.Remove(highlight);
            PushToRedoStack(actionToUndo);
          }
          break;

        case ActiveReadingTool.Pen:
          if (actionToUndo.Parameters.Length == 1)
          {
            ScribbleCollection collection = (ScribbleCollection)actionToUndo.Parameters[0];
            inkCScribble.Strokes.Remove(collection.Scribbles[0]);
            ActualDocument[ActualPage].ScribblingCollections.Remove(collection);
            PushToRedoStack(actionToUndo);
          }
          else if (actionToUndo.Parameters.Length == 2)
          {
            ScribbleCollection collection = (ScribbleCollection)actionToUndo.Parameters[0];
            Stroke stroke = (Stroke)actionToUndo.Parameters[1];
            inkCScribble.Strokes.Remove(stroke);
            collection.Scribbles.Remove(stroke);
            PushToRedoStack(actionToUndo);
          }
          break;

        case ActiveReadingTool.Eraser:
          if (actionToUndo.Parameters[0] is ScribbleCollection)
          {
            if (actionToUndo.Parameters.Length == 1)
            {
              ScribbleCollection collection = (ScribbleCollection)actionToUndo.Parameters[0];
              inkCScribble.Strokes.Add(collection.Scribbles[0]);
              ActualDocument[ActualPage].ScribblingCollections.Add(collection);
              PushToRedoStack(actionToUndo);
            }
            else
            {
              ScribbleCollection collection = (ScribbleCollection)actionToUndo.Parameters[0];
              Stroke stroke = (Stroke)actionToUndo.Parameters[1];
              inkCScribble.Strokes.Add(stroke);
              collection.Scribbles.Add(stroke);
              PushToRedoStack(actionToUndo);
            }
          }
          else if (actionToUndo.Parameters[0] is Highlight)
          {
            Highlight highlight = (Highlight)actionToUndo.Parameters[0];
            AddHighlight(highlight.Line, cHighlights);
            ActualDocument[ActualPage].Highlights.Add(highlight);
            PushToRedoStack(actionToUndo);
          }
          break;
      }
    }

    private void bRedo_Click(object sender, RoutedEventArgs e)
    {
      bRedo.IsChecked = false;
      ProcessRedoRequest();
    }

    private void ProcessRedoRequest()
    {
      if (redoStack.Count == 0)
        return;

      ToolObjectPair actionToRedo = redoStack.Pop();
      switch (actionToRedo.Tool)
      {
        case ActiveReadingTool.Highlighter:
          {
            Highlight highlight = (Highlight)actionToRedo.Parameters[0];
            AddHighlight(highlight.Line, cHighlights);
            ActualDocument[ActualPage].Highlights.Add(highlight);
            PushToUndoStack(actionToRedo, false);
          }
          break;

        case ActiveReadingTool.Pen:
          if (actionToRedo.Parameters.Length == 1)
          {
            ScribbleCollection collection = (ScribbleCollection)actionToRedo.Parameters[0];
            inkCScribble.Strokes.Add(collection.Scribbles[0]);
            ActualDocument[ActualPage].ScribblingCollections.Add(collection);
            PushToUndoStack(actionToRedo, false);
          }
          else if (actionToRedo.Parameters.Length == 2)
          {
            ScribbleCollection collection = (ScribbleCollection)actionToRedo.Parameters[0];
            Stroke stroke = (Stroke)actionToRedo.Parameters[1];
            inkCScribble.Strokes.Add(stroke);
            collection.Scribbles.Add(stroke);
            PushToUndoStack(actionToRedo, false);
          }
          break;

        case ActiveReadingTool.Eraser:
          if (actionToRedo.Parameters[0] is ScribbleCollection)
          {
            if (actionToRedo.Parameters.Length == 1)
            {
              ScribbleCollection collection = (ScribbleCollection)actionToRedo.Parameters[0];
              inkCScribble.Strokes.Remove(collection.Scribbles[0]);
              ActualDocument[ActualPage].ScribblingCollections.Remove(collection);
              PushToUndoStack(actionToRedo, false);
            }
            else
            {
              ScribbleCollection collection = (ScribbleCollection)actionToRedo.Parameters[0];
              Stroke stroke = (Stroke)actionToRedo.Parameters[1];
              inkCScribble.Strokes.Remove(stroke);
              collection.Scribbles.Remove(stroke);
              PushToUndoStack(actionToRedo, false);
            }
          }
          else if (actionToRedo.Parameters[0] is Highlight)
          {
            Highlight highlight = (Highlight)actionToRedo.Parameters[0];
            RemoveHighlight(highlight.Line, cHighlights);
            ActualDocument[ActualPage].Highlights.Remove(highlight);
            PushToUndoStack(actionToRedo, false);
          }
          break;
      }
    }

    protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
    {
      base.OnPreviewMouseDown(e);
      if (Core.Device.State != StackingState.StackedOnTop)
        return;
      if (!bCopySelected.IsChecked.Value)
        return;
      if (e.ChangedButton != MouseButton.Left)
        return;

      Core.Device.SendTouchEvent(GetMousePositionInDocument(), TouchAction.Down);
    }

    protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
    {
      base.OnPreviewMouseUp(e);
      if (Core.Device.State != StackingState.StackedOnTop)
        return;
      if (!bCopySelected.IsChecked.Value)
        return;
      if (e.ChangedButton != MouseButton.Left)
        return;

      Core.Device.SendTouchEvent(GetMousePositionInDocument(), TouchAction.Up);
    }

    protected override void OnPreviewMouseMove(MouseEventArgs e)
    {
      base.OnPreviewMouseMove(e);
      if (Core.Device.State != StackingState.StackedOnTop)
        return;
      if (!bCopySelected.IsChecked.Value)
        return;
      if (e.LeftButton != MouseButtonState.Pressed)
        return;

      Core.Device.SendTouchEvent(GetMousePositionInDocument(), TouchAction.Move);
    }

    void Device_StackingChanged(object sender, StackingEventArgs e)
    {
      Dispatcher.Invoke(DispatcherPriority.Background,
      (Action)delegate()
      {
        UIThreadDevice_StackingChanged(sender, e);
      });
    }

    void UIThreadDevice_StackingChanged(object sender, StackingEventArgs e)
    {
      if (e.State == StackingState.NotStacked)
      {
        if (inkCScribble.Strokes.Contains(stackingSelectionStroke))
          inkCScribble.Strokes.Remove(stackingSelectionStroke);
        inkCScribble.DefaultDrawingAttributes.Color = previousColor;
      }
      else if (e.State == StackingState.StackedBelow || e.State == StackingState.StackedOnTop)
      {
        CurrentTool = ActiveReadingTool.None;
        previousColor = inkCScribble.DefaultDrawingAttributes.Color;
        inkCScribble.DefaultDrawingAttributes.Color = Color.FromRgb(0, 0, 255);
      }
    }

    private Stroke stackingSelectionStroke = null;
    private Color previousColor = Color.FromRgb(0, 0, 0);
    void Device_StackingTouchEvent(object sender, StackingTouchEventArgs e)
    {
      Dispatcher.Invoke(DispatcherPriority.Background,
      (Action)delegate()
      {
        UIThreadDevice_StackingTouchEvent(sender, e);
      });
    }

    private void UIThreadDevice_StackingTouchEvent(object sender, StackingTouchEventArgs e)
    {
      if (e.Action == TouchAction.Down)
      {
        StylusPointCollection points = new StylusPointCollection();
        points.Add(new StylusPoint(e.Location.X, e.Location.Y));
        stackingSelectionStroke = new Stroke(points, inkCScribble.DefaultDrawingAttributes);
        inkCScribble.Strokes.Add(stackingSelectionStroke);
      }
      else if (e.Action == TouchAction.Move)
      {
        stackingSelectionStroke.StylusPoints.Add(new StylusPoint(e.Location.X, e.Location.Y));
        foreach (ScribbleCollection collection in ActualDocument[ActualPage].ScribblingCollections)
        {
          foreach (Stroke stroke in collection.Scribbles)
          {
            if (!stroke.HitTest(e.Location))
              continue;
            Synch.SendContent(ActualDocument.ID, ActualPage, stroke);
          }
        }

        var line = cHighlights.InputHitTest(e.Location);
        if (line == null || !(line is Line))
          return;
        Highlight highlight = (Highlight)ActualDocument[ActualPage].Highlights.SingleOrDefault(tmp => (tmp as Highlight).Line == line);
        if (highlight == null)
          return;
        Synch.SendContent(ActualDocument.ID, ActualPage, highlight);
      }
      else if (e.Action == TouchAction.Up)
      {
        if (inkCScribble.Strokes.Contains(stackingSelectionStroke))
          inkCScribble.Strokes.Remove(stackingSelectionStroke);
      }
    }

    private void bUnStack_Click(object sender, RoutedEventArgs e)
    {
      Core.Device.UnStack();
    }

    void Synch_SynchContent(object sender, SynchContentEventArgs e)
    {
      Dispatcher.Invoke(DispatcherPriority.Background,
        (Action)delegate()
        {
          UIThreadSynch_SynchContent(e);
        });
    }

    private void UIThreadSynch_SynchContent(SynchContentEventArgs e)
    {
      if (e.Type == SynchContentEventType.RequestCopyCurrentPage)
      {
        //Sends the highlights, scribbles and notes in the current page
        SendAll(ActualPage);
      }
      else if (e.Type == SynchContentEventType.RequestCopyAll)
      {
        for (int pageIndex = 0; pageIndex < ActualDocument.Pages.Length; pageIndex++)
          SendAll(pageIndex);
      }
      else if (e.Type == SynchContentEventType.Highlight)
      {
        if (e.DocumentID != ActualDocument.ID)
          return;
        if (e.PageIndex < 0 || e.PageIndex >= ActualDocument.Pages.Length)
          return;

        Highlight highlight = e.Content as Highlight;
        var exists = ActualDocument[e.PageIndex].Highlights.FirstOrDefault(tmp => tmp.ID == highlight.ID);
        if (exists != null)
          return;

        ActualDocument[e.PageIndex].Highlights.Add(highlight);
        if (e.PageIndex == ActualPage)
          AddHighlight(highlight.Line, cHighlights);
        PushToUndoStack(ActiveReadingTool.Highlighter, highlight);
      }
      else if (e.Type == SynchContentEventType.Stroke)
      {
        if (e.DocumentID != ActualDocument.ID)
          return;
        if (e.PageIndex < 0 || e.PageIndex >= ActualDocument.Pages.Length)
          return;

        Stroke stroke = e.Content as Stroke;
        Guid collectionID = new Guid((String)stroke.GetPropertyData(strokeCollectionPropertyID));
        ScribbleCollection collection = (ScribbleCollection)ActualDocument[e.PageIndex].ScribblingCollections.FirstOrDefault(tmp => tmp.ID == collectionID);
        if (collection == null)
        {
          collection = new ScribbleCollection() { ID = collectionID, Scribbles = new StrokeCollection() };
          ActualDocument[e.PageIndex].ScribblingCollections.Add(collection);
        }

        Guid strokeID = Guid.Parse(stroke.GetPropertyData(strokeObjectPropertyID) as String);
        var strokeExist = collection.Scribbles.FirstOrDefault(tmp => Guid.Parse(tmp.GetPropertyData(strokeObjectPropertyID) as String) == strokeID);
        if (strokeExist != null)
          return;

        collection.Scribbles.Add(stroke);
        if (e.PageIndex == ActualPage)
          inkCScribble.Strokes.Add(stroke);
        PushToUndoStack(ActiveReadingTool.Pen, collection, stroke);
      }
      else if (e.Type == SynchContentEventType.Note)
      {
        if (e.DocumentID != ActualDocument.ID)
          return;
        if (e.PageIndex < 0 || e.PageIndex >= ActualDocument.Pages.Length)
          return;

        Note note = e.Content as Note;
        var exists = ActualDocument[e.PageIndex].Annotations.FirstOrDefault(tmp => tmp.ID == note.ID);
        if (exists != null)
          return;

        if (e.PageIndex == ActualPage)
          AddNote(note);
        ActualDocument[e.PageIndex].Annotations.Add(note);
      }
    }

    private void SendAll(int pageIndex)
    {
      //highlights
      foreach (Highlight highlight in ActualDocument[pageIndex].Highlights)
        Synch.SendContent(ActualDocument.ID, pageIndex, highlight);

      //scribbles
      foreach (ScribbleCollection collection in ActualDocument[pageIndex].ScribblingCollections)
        Synch.SendContent(ActualDocument.ID, pageIndex, collection);

      //notes
      foreach (Note note in ActualDocument[pageIndex].Annotations)
        Synch.SendContent(ActualDocument.ID, pageIndex, note);
    }

    private void bCopyAll_Click(object sender, RoutedEventArgs e)
    {
      Synch.RequestCopyAll();
    }

    private void bCopyCurrentPage_Click(object sender, RoutedEventArgs e)
    {
      Synch.RequestCopyCurrentPage();
    }

    private void meVideo_MediaEnded(object sender, RoutedEventArgs e)
    {
      //Console.WriteLine("meVideo_MediaEnded");
    }

    private void meVideo_MediaFailed(object sender, ExceptionRoutedEventArgs e)
    {
      //Console.WriteLine("meVideo_MediaFailed");
    }

    private void meVideo_MediaOpened(object sender, RoutedEventArgs e)
    {
      //Console.WriteLine("meVideo_MediaOpened");
    }

  }

}