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
using System.Collections.ObjectModel;
using System.IO;
using Ubicomp.Utils.NET.MTF;
using UofM.HCI.tPad.App.PhotoAlbum.Network;
using System.Windows.Threading;
using System.Threading;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

namespace UofM.HCI.tPad.App.PhotoAlbum
{
  /// <summary>
  /// Interaction logic for PhotoAlbumApp.xaml
  /// </summary>
  public partial class PhotoAlbumApp : UserControl, ITPadApp, INotifyPropertyChanged, ITransportListener
  {

    public const int BUFFER_SIZE = 10240;

    public event BoolEventHandler IsTopApp;
    public event RequestUserFocus RequestFocus;
    public event EventHandler Closed;
    public event PropertyChangedEventHandler PropertyChanged;

    public TPadCore Core { get; set; }
    public ITPadAppContainer Container { get; set; }
    public ITPadAppController Controller { get; set; }

    public Dictionary<string, Object> Context { get { return null; } }

    public ObservableCollection<String> LocalPhotos { get; set; }
    public ObservableCollection<String> ReceivedPhotos { get; set; }
    private BackgroundWorker imageSender = new BackgroundWorker();

    private DraggingDirection dragging = DraggingDirection.None;
    public DraggingDirection Dragging
    {
      get { return dragging; }
      set
      {
        dragging = value;
        OnPropertyChanged("Dragging");
      }
    }

    private String networkOperation = String.Empty;
    public String NetworkOperation
    {
      get { return networkOperation; }
      set
      {
        networkOperation = value;

        if (networkOperation == String.Empty)
          Canvas.SetZIndex(gNetworkOperation, -1);
        else
          Canvas.SetZIndex(gNetworkOperation, 10);
        
        OnPropertyChanged("NetworkOperation");
      }
    }

    private double networkProgress = 0.0;
    public double NetworkProgress
    {
      get { return networkProgress; }
      set
      {
        networkProgress = value;
        OnPropertyChanged("NetworkProgress");
      }
    }

    public PhotoAlbumApp(TPadCore core, ITPadAppContainer container, ITPadAppController controller)
    {
      Core = core;
      Container = container;
      Controller = controller;

      LocalPhotos = new ObservableCollection<String>();
      ReceivedPhotos = new ObservableCollection<String>();

      InitializeComponent();

      TransportComponent.Instance.TransportListeners.Add(this);
      if (!TransportMessageExporter.Exporters.ContainsKey(PhotoMessage.MessageID))
        TransportMessageExporter.Exporters.Add(PhotoMessage.MessageID, new PhotoMessageExporter());
      if (!TransportMessageImporter.Importers.ContainsKey(PhotoMessage.MessageID))
        TransportMessageImporter.Importers.Add(PhotoMessage.MessageID, new PhotoMessageImporter());

      LoadPhotos();
      imageSender.WorkerReportsProgress = true;
      imageSender.WorkerSupportsCancellation = true;
      imageSender.DoWork += new DoWorkEventHandler(imageSender_DoWork);
      imageSender.ProgressChanged += new ProgressChangedEventHandler(imageSender_ProgressChanged);
      imageSender.RunWorkerCompleted += new RunWorkerCompletedEventHandler(imageSender_RunWorkerCompleted);

      Core.Device.StackingChanged += new StackingChangedEventHandler(Device_StackingChanged);
      Core.Device.StackingTouchEvent += new StackingTouchEventEventHandler(Device_StackingTouchEvent);
    }

    public void Close()
    {
      if (TransportComponent.Instance.TransportListeners.Contains(this))
        TransportComponent.Instance.TransportListeners.Remove(this);
      if (TransportMessageExporter.Exporters.ContainsKey(PhotoMessage.MessageID))
        TransportMessageExporter.Exporters.Remove(PhotoMessage.MessageID);
      if (TransportMessageImporter.Importers.ContainsKey(PhotoMessage.MessageID))
        TransportMessageImporter.Importers.Remove(PhotoMessage.MessageID);

      if (Closed != null)
        Closed(this, EventArgs.Empty);
    }

    public void OnPropertyChanged(String name)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(name));
    }

    private void LoadPhotos()
    {
      LocalPhotos.Clear();
      String path = String.Format("{0}\\Device{1}\\Photos\\", Environment.CurrentDirectory, Core.Device.ID);
      var photos = System.IO.Directory.EnumerateFiles(path, "*.jpg");
      foreach (String photo in photos)
        LocalPhotos.Add(photo);
    }

    public void LoadInitContext(Dictionary<string, Object> init) { }

    private void Image_MouseUp(object sender, MouseButtonEventArgs e)
    {
      imgZoom.DataContext = (sender as Image).DataContext;
      imgZoom.Source = (sender as Image).Source;
      gZoom.Visibility = System.Windows.Visibility.Visible;
      Canvas.SetZIndex(gZoom, 10);
    }

    private void btnCloseZoom_Click(object sender, RoutedEventArgs e)
    {
      imgZoom.DataContext = null;
      imgZoom.Source = null;
      gZoom.Visibility = System.Windows.Visibility.Collapsed;
      Canvas.SetZIndex(gZoom, -1);
    }

    void Device_StackingChanged(object sender, StackingEventArgs e)
    {
      if (e.State == StackingState.StackedOnTop || e.State == StackingState.StackedBelow)
      {
      }
      if (e.State == StackingState.NotStacked)
      {
      }
    }

    private Point startPos, lastSentPos;
    protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
    {
      base.OnPreviewMouseDown(e);
      if (Core.Device.State == StackingState.NotStacked || Core.Device.State == StackingState.StackedBelow)
        return;

      startPos = Mouse.GetPosition(this);
      if (startPos.Y < ActualHeight / 2)
        return;

      Core.Device.SendTouchEvent(startPos, TouchAction.Down);
    }

    protected override void OnPreviewMouseMove(MouseEventArgs e)
    {
      base.OnPreviewMouseMove(e);
      if (Core.Device.State == StackingState.NotStacked || Core.Device.State == StackingState.StackedBelow)
        return;

      Point mousePos = Mouse.GetPosition(this);
      if (mousePos.Y < ActualHeight / 2)
      {
        if (Dragging == DraggingDirection.None)
        {
          Vector diff = startPos - mousePos;
          if (e.LeftButton == MouseButtonState.Pressed &&
            (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
          {
            // Get the dragged ListViewItem
            DependencyObject dragSource = null;
            String picturePath = String.Empty;
            ListBoxItem container = FindAnchestor<ListBoxItem>((DependencyObject)e.OriginalSource);
            if (container != null)
            {
              dragSource = container;
              picturePath = (String)lbLocalPhotos.ItemContainerGenerator.ItemFromContainer(container);
            }
            else if (e.OriginalSource is Image)
            {
              dragSource = (e.OriginalSource as Image);
              picturePath = (e.OriginalSource as Image).DataContext as String;
            }

            if (picturePath == null || picturePath == String.Empty)
              return;

            // Initialize the drag & drop operation
            Dragging = DraggingDirection.TopToBelow;
            DataObject dragData = new DataObject("photoURI", picturePath);
            DragDrop.DoDragDrop(dragSource, dragData, DragDropEffects.Move);
            Dragging = DraggingDirection.None;
          }
        }
        return;
      }

      if (startPos.X != 0 && startPos.Y != 0 && startPos.Y >= ActualHeight / 2)
      {
        Vector diff = lastSentPos - mousePos;
        if (Math.Abs(diff.X) < 10 && Math.Abs(diff.Y) < 10)
          return;

        lastSentPos = mousePos;
        Core.Device.SendTouchEvent(mousePos, TouchAction.Move);
      }
    }

    protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
    {
      base.OnPreviewMouseUp(e);
      startPos = lastSentPos = new Point(0, 0);
      if (Core.Device.State == StackingState.NotStacked || Core.Device.State == StackingState.StackedBelow)
        return;

      Point location = Mouse.GetPosition(this);
      Core.Device.SendTouchEvent(location, TouchAction.Up);
    }

    private String pictureDraggingBelow = String.Empty;
    void Device_StackingTouchEvent(object sender, StackingTouchEventArgs e)
    {
      Matrix topToBelowMatrix = Matrix.Identity;
      topToBelowMatrix.ScaleAt(-1, -1, ActualWidth / 2, ActualHeight / 2);

      if (e.Action == TouchAction.Down)
      {
        startPos = topToBelowMatrix.Transform(new Point(e.Location.X, e.Location.Y));
        return;
      }

      if (e.Action == TouchAction.Move && pictureDraggingBelow == String.Empty)
      {
        Point mousePos = topToBelowMatrix.Transform(new Point(e.Location.X, e.Location.Y));
        Vector diff = startPos - mousePos;
        if ((Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
        {
          var result = VisualTreeHelper.HitTest(this, mousePos);

          // Get the dragged ListViewItem
          String picturePath = String.Empty;
          ListBoxItem container = FindAnchestor<ListBoxItem>((DependencyObject)result.VisualHit);
          if (container != null)
            picturePath = (String) lbLocalPhotos.ItemContainerGenerator.ItemFromContainer(container);
          else if (result.VisualHit is Image)
            picturePath = (result.VisualHit as Image).DataContext as String;

          // Initialize the drag & drop operation
          pictureDraggingBelow = picturePath;
          Core.Device.SendMessage(new PhotoMessage() { Type = PhotoMessage.PhotoMessageType.DragStarted });
        }
        return;
      }

      if (e.Action == TouchAction.Up)
      {
        Core.Device.SendMessage(new PhotoMessage() { Type = PhotoMessage.PhotoMessageType.DragFinished });

        Point mousePos = topToBelowMatrix.Transform(new Point(e.Location.X, e.Location.Y));
        if (mousePos.Y < ActualHeight / 2) //the click was in the own area
        {
          Vector diff = startPos - mousePos;
          if ((Math.Abs(diff.X) <= SystemParameters.MinimumHorizontalDragDistance &&
            Math.Abs(diff.Y) <= SystemParameters.MinimumVerticalDragDistance))
          {
            //was click and should for forward it to the right control
            var target = VisualTreeHelper.HitTest(this, mousePos);
            Image picture = FindAnchestor<Image>((DependencyObject)target.VisualHit);
            if (picture != null)
              Image_MouseUp(picture, null);

            Button button = FindAnchestor<Button>((DependencyObject)target.VisualHit);
            if (button != null)
            {
              ButtonAutomationPeer peer = new ButtonAutomationPeer(button);
              IInvokeProvider invoke = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
              invoke.Invoke();
            }
          }
        }

        String picturePath = pictureDraggingBelow;
        pictureDraggingBelow = String.Empty;
        startPos = new Point(0, 0);

        // the click was in the "drop here area" to send to the device on top
        if (picturePath != String.Empty)
          SendPicture(Core.Device.DeviceOnTop, picturePath);
      }
    }

    private static T FindAnchestor<T>(DependencyObject current) where T : DependencyObject
    {
      do
      {
        if (current is T)
        {
          return (T)current;
        }
        current = VisualTreeHelper.GetParent(current);
      }
      while (current != null);
      return null;
    }

    public enum DraggingDirection { None, TopToBelow, BelowToTop };

    private void gDrop_DragEnter(object sender, DragEventArgs e)
    {
      gDropRectTTB.Background = Brushes.Red;
    }

    private void gDrop_DragLeave(object sender, DragEventArgs e)
    {
      gDropRectTTB.Background = Brushes.Black;
    }

    private void gDropFromBelowToTop_MouseEnter(object sender, MouseEventArgs e)
    {
      gDropRectBTT.Background = Brushes.Red;
    }

    private void gDropFromBelowToTop_MouseLeave(object sender, MouseEventArgs e)
    {
      gDropRectBTT.Background = Brushes.Black;
    }

    private void gDrop_Drop(object sender, DragEventArgs e)
    {
      String pictureName = (String)e.Data.GetData("photoURI");
      SendPicture(Core.Device.DeviceBelow, pictureName);
    }

    private void SendPicture(int remoteDevide, string picturePath)
    {
      NetworkOperation = "Sending File";
      imageSender.RunWorkerAsync(picturePath);
    }

    public int MessageType
    {
      get { return PhotoMessage.MessageID; }
    }

    private List<PhotoMessage> parts = new List<PhotoMessage>();
    public void MessageReceived(TransportMessage message, string rawMessage)
    {
      if (message.MessageSource.ResourceId == Core.Device.TMessageEventSource.ResourceId)
        return;

      PhotoMessage pMessage = (PhotoMessage)message.MessageData;
      if (pMessage.Type == PhotoMessage.PhotoMessageType.FilePart)
      {
        if (parts.Count != 0 && parts[0].Transaction != pMessage.Transaction)
          parts.Clear();

        parts.Add(pMessage);
        Dispatcher.Invoke(DispatcherPriority.Render,
        (Action)delegate()
        {
          NetworkOperation = "Receiving File";
          NetworkProgress = (pMessage.ActualPart / pMessage.TotalParts) * 100;
        });

        if (parts.Count == pMessage.TotalParts)
          SaveReceivedFile();
      }
      else if (pMessage.Type == PhotoMessage.PhotoMessageType.DragStarted)
      {
        Dragging = DraggingDirection.BelowToTop;
      }
      else if (pMessage.Type == PhotoMessage.PhotoMessageType.DragFinished)
      {
        Dragging = DraggingDirection.None;
      }
    }

    private void SaveReceivedFile()
    {
      parts.Sort((part1, part2) => part1.ActualPart - part2.ActualPart);

      string fileName = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".jpg";
      FileStream picture = File.OpenWrite(fileName);
      foreach (PhotoMessage part in parts)
        picture.Write(part.Content, 0, part.ContentSize);
      picture.Close();
      parts.Clear();

      Dispatcher.Invoke(DispatcherPriority.Render,
        (Action)delegate()
        {
          ReceivedPhotos.Add(fileName);
          NetworkProgress = 100;
          NetworkOperation = String.Empty;
        });
    }

    void imageSender_DoWork(object sender, DoWorkEventArgs e)
    {
      BackgroundWorker worker = sender as BackgroundWorker;
      String picturePath = e.Argument as String;
      if (!File.Exists(picturePath))
        return;

      Guid transaction = Guid.NewGuid();
      FileStream picture = File.OpenRead(picturePath);
      long lenght = picture.Length;
      int totalParts = (int)(lenght / BUFFER_SIZE);
      if (lenght % BUFFER_SIZE != 0)
        totalParts++;

      for (int part = 0; part < totalParts; part++)
      {
        PhotoMessage message = new PhotoMessage();
        message.Type = PhotoMessage.PhotoMessageType.FilePart;
        message.Transaction = transaction;
        message.FileName = System.IO.Path.GetFileName(picturePath);
        message.TotalParts = totalParts;
        message.ActualPart = part;

        if (part < totalParts - 1)
          message.ContentSize = BUFFER_SIZE;
        else
          message.ContentSize = (int)(lenght % 10240);

        message.Content = new byte[message.ContentSize];
        picture.Read(message.Content, 0, message.ContentSize);

        Core.Device.SendMessage(message);
        worker.ReportProgress((message.ActualPart / message.TotalParts) * 100, null);
        Thread.Sleep(20);
      }
    }

    void imageSender_ProgressChanged(object sender, ProgressChangedEventArgs e)
    {
      NetworkProgress = e.ProgressPercentage;
    }

    void imageSender_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
      NetworkOperation = String.Empty;
    }

    private void btnStopNetworkOperation_Click(object sender, RoutedEventArgs e)
    {
      if (imageSender.IsBusy)
        imageSender.CancelAsync();
      NetworkOperation = String.Empty;
      parts.Clear();
    }
  }
}
