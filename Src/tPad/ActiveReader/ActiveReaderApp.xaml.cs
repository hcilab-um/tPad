﻿﻿using System;
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

    public TPadPage ActualPageObject
    {
      get
      {
        if (ActualDocument == null || ActualPage == -1 || ActualDocument.Pages == null || ActualDocument.Pages.Length <= ActualPage)
          return null;
        return ActualDocument.Pages[ActualPage];
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

    public ActiveReaderApp(String documentPDF, ITPadAppContainer container)
    {
      Device = TPadCore.Instance.Device;
      Profile = TPadCore.Instance.Profile;

      WidthScalingFactor = 1;
      HeightScalingFactor = 1;
      
      ActualPage = -1;
      ActualDocument = null;
      
      Container = container;
      PdfHelper = new PDFContentHelper(documentPDF);

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
      TPadCore.Instance.Device.StackingChanged += new StackingChangedEventHandler(Device_StackingChanged);
      TPadCore.Instance.Device.FlippingChanged += new FlippingChangedEventHandler(Device_FlippingChanged);
      TPadCore.Instance.Device.RegistrationChanged += new RegistrationChangedEventHandler(Device_RegistrationChanged);

      WidthScalingFactor = ActualWidth / Profile.Resolution.Width;
      HeightScalingFactor = ActualHeight / Profile.Resolution.Height;
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
          foreach (Highlight element in document.Pages[pageIndex].Highlights)
          {
            Highlight highlight = (Highlight)element;
            highlight.line.MouseDown += cHighlights_MouseDown;
            highlight.line.MouseMove += cHighlights_MouseMove;
            highlight.line.MouseUp += cHighlights_MouseUp;
            cHighlights.Children.Add(highlight.line);
          }

          //Loads search results for this page
          foreach (Highlight element in document.Pages[pageIndex].SearchResults)
          {
            Highlight searchHighlight = (Highlight)element;
            searchHighlight.line.MouseDown += cHighlights_MouseDown;
            searchHighlight.line.MouseMove += cHighlights_MouseMove;
            searchHighlight.line.MouseUp += cHighlights_MouseUp;
            cSearchResults.Children.Add(searchHighlight.line);
          }

          //Loads notes for this page
          foreach (Note element in document.Pages[pageIndex].Annotations)
          {
            Note note = (Note)element;
            cHighlights.Children.Add(note.annotation);
            cHighlights.Children.Add(note.icon);
          }

          //Loads scribbles for this page
          foreach (Scribble element in document.Pages[pageIndex].Scribblings)
          {
            Scribble note = (Scribble)element;
            cHighlights.Children.Add(note.scribbling);
            cHighlights.Children.Add(note.icon);
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

    private void clearSearch()
    {
      foreach (TPadPage documentPage in ActualDocument.Pages)
      {
        documentPage.SearchResults.Clear();
      }
      cSearchResults.Children.Clear();
    }
    private void search(String word, int page)
    {
      clearSearch();

      List<ContentLocation> pageSearch = PdfHelper.ContentToPixel(word, page, gAnchoredLayers.ActualWidth, gAnchoredLayers.ActualHeight);
      
      foreach (ContentLocation content in pageSearch)
      {
        Highlight resultHL = new Highlight();
        resultHL.line = new Line() { Stroke = Brushes.Blue, Opacity = 0.5, StrokeThickness = content.ContentBounds.Height };
        resultHL.line.X1 = content.ContentBounds.Left;
        resultHL.line.Y1 = content.ContentBounds.Top + content.ContentBounds.Height / 2;
        resultHL.line.X2 = content.ContentBounds.Right;
        resultHL.line.Y2 = content.ContentBounds.Top + content.ContentBounds.Height / 2;
        if (content.PageIndex == ActualPage)
          cSearchResults.Children.Add(resultHL.line);
        ActualDocument.Pages[content.PageIndex].SearchResults.Add(resultHL);
      }
    }

    private bool isHighlighting = false;
    private Point lastPosition;
    private Highlight newHighlight = new Highlight();
    private Highlight currentHighlight = new Highlight();
    private bool isSomething2Hide = false;
    private bool isSenderLine = false;
    private void cHighlights_MouseDown(object sender, MouseButtonEventArgs e)
    {
      if (e.LeftButton == MouseButtonState.Pressed && e.RightButton == MouseButtonState.Released)
      {
        isHighlighting = true;
        lastPosition = Mouse.GetPosition(gAnchoredLayers);

        newHighlight = new Highlight();
        newHighlight.line = new Line { Stroke = Brushes.YellowGreen, Opacity = 0.5, StrokeThickness = 18 };
        newHighlight.line.MouseDown += cHighlights_MouseDown;
        newHighlight.line.MouseMove += cHighlights_MouseMove;
        newHighlight.line.MouseUp += cHighlights_MouseUp;
        newHighlight.line.X1 = lastPosition.X;
        newHighlight.line.Y1 = lastPosition.Y;
        newHighlight.line.X2 = lastPosition.X;
        newHighlight.line.Y2 = lastPosition.Y;
        cHighlights.Children.Add(newHighlight.line);

        contextMenu.Visibility = Visibility.Hidden;
        tpKeyboard.Visibility = Visibility.Hidden;

        isSomething2Hide = false;
        foreach (Note element in ActualDocument.Pages[ActualPage].Annotations)
        {
          if (element.annotation.Visibility == Visibility.Visible)
          {
            isSomething2Hide = true;
            element.annotation.Visibility = Visibility.Hidden;
          }
        }

        foreach (Scribble element in ActualDocument.Pages[ActualPage].Scribblings)
        {
          if (element.scribbling.Visibility == Visibility.Visible)
          {
            isSomething2Hide = true;
            element.scribbling.Visibility = Visibility.Hidden;
          }
        }

        if (sender.GetType() == typeof(Line))
        {
          isSenderLine = true;
          currentHighlight.line = (Line)sender;
        }
        else isSenderLine = false;
      }
    }

    private float minlength_Highlight = 10;
    private void cHighlights_MouseUp(object sender, MouseButtonEventArgs e)
    {
      if (!isHighlighting)
        return;

      isHighlighting = false;
      Point newPosition = Mouse.GetPosition(gAnchoredLayers);
      newHighlight.line.X2 = newPosition.X;
      newHighlight.line.Y2 = newPosition.Y;

      Vector lineVector = new Vector(newHighlight.line.X2 - newHighlight.line.X1, newHighlight.line.Y2 - newHighlight.line.Y1);
      if (lineVector.Length > minlength_Highlight)
        ActualDocument.Pages[ActualPage].Highlights.Add(newHighlight);
      else //It was just a click to bring up the contextual menu
      {
        cHighlights.Children.Remove(newHighlight.line);

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
      if (!isHighlighting)
        return;

      Point newPosition = Mouse.GetPosition(gAnchoredLayers);
      newHighlight.line.X2 = newPosition.X;
      newHighlight.line.Y2 = newPosition.Y;
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
        contextMenu.Visibility = Visibility.Visible;
        cm_deleteItem.Visibility = Visibility.Collapsed;

        if (isSenderLine)
          cm_deleteItem.Visibility = Visibility.Visible;
      }
    }

    private void CMDelete_Click(object sender, RoutedEventArgs e)
    {
      cHighlights.Children.Remove(currentHighlight.line);
      ActualDocument.Pages[ActualPage].Highlights.Remove(currentHighlight);
    }

    private void CMSearch_Click(object sender, RoutedEventArgs e)
    {
      search(SearchTerm, -1);
      SearchTerm = String.Empty;
      bSearch.IsChecked = true;
    }

    private void CMAnnotation_Click(object sender, RoutedEventArgs e)
    {
      //show keyboard and clean result
      tpKeyboard.Visibility = Visibility.Visible;
      tpKeyboard.ResultClear();

      Note newNote = new Note();
      newNote.annotation = new TextBox
      {
        BorderBrush = Brushes.Goldenrod,
        Background = Brushes.LemonChiffon,
        Width = (int)iDocument.Width / 7,
        Height = (int)iDocument.Width / 7,
        TextWrapping = TextWrapping.Wrap
      };

      //rotate sticky note
      //RotateTransform rotation = new RotateTransform(Device.Location.RotationAngle, newNote.annotation.Width * 0.5, newNote.annotation.Height * 0.5);
      //newNote.annotation.RenderTransform = rotation;

      newNote.annotation.Margin = new Thickness(lastPosition.X, lastPosition.Y + 10, 0, 0);
      newNote.annotation.PreviewMouseDown += Note_PreviewMouseDown;
      newNote.annotation.PreviewMouseUp += Note_PreviewMouseUp;
      newNote.annotation.PreviewMouseMove += Note_PreviewMouseMove;

      newNote.icon = new Image { Width = (int)iDocument.Width / 30, Height = (int)iDocument.Width / 25 };
      string strUri2 = (Environment.CurrentDirectory + "\\ICON.png");
      newNote.icon.Source = new BitmapImage(new Uri(strUri2));
      newNote.icon.Margin = new Thickness(lastPosition.X, lastPosition.Y - newNote.icon.Height, 0, 0);
      newNote.icon.MouseDown += Icon_MouseDown;

      cHighlights.Children.Add(newNote.annotation);
      cHighlights.Children.Add(newNote.icon);
      ActualDocument.Pages[ActualPage].Annotations.Add(newNote);

      //Update current note
      ActualNote = newNote;
    }

    private void CMScribble_Click(object sender, RoutedEventArgs e)
    {
      Scribble newScribble = new Scribble();
      newScribble.scribbling = new InkCanvas()
      {
        Background = Brushes.Beige,
        Width = (int)iDocument.Width / 7,
        Height = (int)iDocument.Width / 7,
      };

      newScribble.scribbling.Margin = new Thickness(lastPosition.X, lastPosition.Y + 10, 0, 0);

      newScribble.icon = new Image { Width = (int)iDocument.Width / 30, Height = (int)iDocument.Width / 25 };
      string strUri2 = (Environment.CurrentDirectory + "\\ICON.png");
      newScribble.icon.Source = new BitmapImage(new Uri(strUri2));
      newScribble.icon.Margin = new Thickness(lastPosition.X, lastPosition.Y - newScribble.icon.Height, 0, 0);
      newScribble.icon.MouseDown += ScribbleIcon_MouseDown;

      cHighlights.Children.Add(newScribble.scribbling);
      cHighlights.Children.Add(newScribble.icon);
      ActualDocument.Pages[ActualPage].Scribblings.Add(newScribble);
    }


    private void Icon_MouseDown(object sender, MouseButtonEventArgs e)
    {
      tpKeyboard.Visibility = Visibility.Hidden;
      foreach (Note element in ActualDocument.Pages[ActualPage].Annotations)
      {
        if (element.icon == (Image)sender)
          ActualNote = element;
      }

      if (e.LeftButton == MouseButtonState.Pressed && e.RightButton == MouseButtonState.Released)
      {
        if (ActualNote.annotation.Visibility == Visibility.Hidden)
          ActualNote.annotation.Visibility = Visibility.Visible;
        else
          ActualNote.annotation.Visibility = Visibility.Hidden;
      }
    }

    private Scribble ActualScribble;
    private void ScribbleIcon_MouseDown(object sender, MouseButtonEventArgs e)
    {
      foreach (Scribble element in ActualDocument.Pages[ActualPage].Scribblings)
      {
        if (element.icon == (Image)sender)
          ActualScribble = element;
      }

      if (e.LeftButton == MouseButtonState.Pressed && e.RightButton == MouseButtonState.Released)
      {
        if (ActualScribble.scribbling.Visibility == Visibility.Hidden)
          ActualScribble.scribbling.Visibility = Visibility.Visible;
        else
          ActualScribble.scribbling.Visibility = Visibility.Hidden;
      }
    }

    private bool isAnnotationMoved = false;
    private bool isAnnotationResized = false;
    static Size defaultNoteSize = new Size(20, 20);
    private void Note_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
      if (e.LeftButton == MouseButtonState.Pressed && e.RightButton == MouseButtonState.Released)
      {
        ActualNote.annotation = (TextBox)sender;

        lastPosition = Mouse.GetPosition(gAnchoredLayers);
        tpKeyboard.Visibility = Visibility.Visible;
        tpKeyboard.ResultClear();
        tpKeyboard.CurrentText.Append(ActualNote.annotation.Text);

        //check if there is a click on bottom right corner of note
        if (lastPosition.X <= (ActualNote.annotation.Margin.Left + ActualNote.annotation.Width) &&
          lastPosition.X >= (ActualNote.annotation.Margin.Left + ActualNote.annotation.Width - 20) &&
          lastPosition.Y <= (ActualNote.annotation.Margin.Top + ActualNote.annotation.Height) &&
          lastPosition.Y >= (ActualNote.annotation.Margin.Top + ActualNote.annotation.Height - 20))
          isAnnotationResized = true;
        else if (lastPosition.X <= (ActualNote.annotation.Margin.Left + ActualNote.annotation.Width) &&
          lastPosition.X >= (ActualNote.annotation.Margin.Left + ActualNote.annotation.Width - 20) &&
          lastPosition.Y <= (ActualNote.annotation.Margin.Top + 20) &&
          lastPosition.Y >= ActualNote.annotation.Margin.Top)
        {
          cHighlights.Children.Remove(ActualNote.annotation);
          cHighlights.Children.Remove(ActualNote.icon);
          ActualDocument.Pages[ActualPage].Annotations.Remove(ActualNote);
          ActualNote.annotation = null;
          ActualNote.icon = null;
          tpKeyboard.Visibility = Visibility.Hidden;
        }
        else
          isAnnotationMoved = true;
      }
    }

    private void Note_PreviewMouseMove(object sender, MouseEventArgs e)
    {
      if (!isAnnotationMoved && !isAnnotationResized)
        return;

      Point currentPosition = Mouse.GetPosition(gAnchoredLayers);
      Vector lineVector = new Vector(currentPosition.X - lastPosition.X,
        currentPosition.Y - lastPosition.Y);
      if (lineVector.Length > 3)
      {
        if (isAnnotationMoved)
          ActualNote.annotation.Margin = new Thickness(currentPosition.X, currentPosition.Y, 0, 0);
        else
        {
          Point noteSize = new Point(currentPosition.X - ActualNote.annotation.Margin.Left,
            currentPosition.Y - ActualNote.annotation.Margin.Top);
          if (noteSize.X >= defaultNoteSize.Width)
            ActualNote.annotation.Width = noteSize.X;
          if (noteSize.Y >= defaultNoteSize.Height)
            ActualNote.annotation.Height = noteSize.Y;
        }
        tpKeyboard.Visibility = Visibility.Hidden;
      }
    }

    private void Note_PreviewMouseUp(object sender, MouseEventArgs e)
    {
      if (!isAnnotationMoved && !isAnnotationResized)
        return;

      isAnnotationMoved = false;
      isAnnotationResized = false;
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
        clearSearch();               
      }
    }

    private void bCopyAndLock_Click(object sender, RoutedEventArgs e)
    {
      if (bCopyAndLock.IsChecked.Value)
        TPadCore.Instance.Registration.Stop();
      else TPadCore.Instance.Registration.Start();
    }

    public void tpKeyboard_EnterKeyPressed(System.Object sender, EventArgs args)
    {
      if (bSearch.IsChecked.Value)
      {
        tpKeyboard.Visibility = Visibility.Hidden;
        search(tpKeyboard.CurrentTextLine.ToString(), -1);
      }
      else if (bHighlight.IsChecked.Value && ActualNote.annotation != null)
        ActualNote.annotation.Text = tpKeyboard.CurrentText.ToString();
    }

    public void tpKeyboard_AlphaNumericKeyPressed(System.Object sender, EventArgs args)
    {
      if (bHighlight.IsChecked.Value && ActualNote.annotation != null && !bSearch.IsChecked.Value)
        ActualNote.annotation.Text = tpKeyboard.CurrentText.ToString();
    }

  }
}