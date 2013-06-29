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

namespace UofM.HCI.tPad.App.PhotoAlbum
{
  /// <summary>
  /// Interaction logic for PhotoAlbumApp.xaml
  /// </summary>
  public partial class PhotoAlbumApp : UserControl, ITPadApp, INotifyPropertyChanged, ITransportListener
  {

    public const int BUFFER_SIZE = 10240;

    public event EventHandler Closed;
    public event PropertyChangedEventHandler PropertyChanged;

    public TPadCore Core { get; set; }
    public ITPadAppContainer Container { get; set; }
    public ITPadAppController Controller { get; set; }

    public Dictionary<string, string> Context { get { return null; } }

    public ObservableCollection<String> LocalPhotos { get; set; }
    public ObservableCollection<String> ReceivedPhotos { get; set; }

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

    public PhotoAlbumApp(TPadCore core, ITPadAppContainer container, ITPadAppController controller)
    {
      Core = core;
      Container = container;
      Controller = controller;

      LocalPhotos = new ObservableCollection<String>();
      ReceivedPhotos = new ObservableCollection<String>();

      InitializeComponent();
      LoadPhotos();

      TransportComponent.Instance.TransportListeners.Add(this);
      if (!TransportMessageExporter.Exporters.ContainsKey(PhotoMessage.MessageID))
        TransportMessageExporter.Exporters.Add(PhotoMessage.MessageID, new PhotoMessageExporter());
      if (!TransportMessageImporter.Importers.ContainsKey(PhotoMessage.MessageID))
        TransportMessageImporter.Importers.Add(PhotoMessage.MessageID, new PhotoMessageImporter());

      Core.Device.StackingChanged += new StackingChangedEventHandler(Device_StackingChanged);
      Core.Device.StackingTouchEvent += new StackingTouchEventEventHandler(Device_StackingTouchEvent);
    }

    public void Close()
    {


      if (Closed != null)
        Closed(this, EventArgs.Empty);
    }

    public void OnPropertyChanged(String name)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(name));
    }

    private void btnClose_Click(object sender, RoutedEventArgs e)
    {
      Close();
    }

    private void LoadPhotos()
    {
      LocalPhotos.Clear();
      String path = String.Format("{0}\\Device{1}\\Photos\\", Environment.CurrentDirectory, Core.Device.ID);
      var photos = System.IO.Directory.EnumerateFiles(path, "*.jpg");
      foreach (String photo in photos)
        LocalPhotos.Add(photo);
    }

    public void LoadInitContext(Dictionary<string, string> init) { }

    private void Image_MouseUp(object sender, MouseButtonEventArgs e)
    {
      imgZoom.Source = (sender as Image).Source;
      gZoom.Visibility = System.Windows.Visibility.Visible;
    }

    private void btnCloseZoom_Click(object sender, RoutedEventArgs e)
    {
      imgZoom.Source = null;
      gZoom.Visibility = System.Windows.Visibility.Collapsed;
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
            ListBoxItem item = FindAnchestor<ListBoxItem>((DependencyObject)e.OriginalSource);
            if (item == null)
              return;

            // Find the data behind the ListViewItem
            var contact = lbLocalPhotos.ItemContainerGenerator.ItemFromContainer(item);

            // Initialize the drag & drop operation
            Dragging = DraggingDirection.TopToBelow;
            DataObject dragData = new DataObject("photoURI", contact);
            DragDrop.DoDragDrop(item, dragData, DragDropEffects.Move);
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
          ListBoxItem item = FindAnchestor<ListBoxItem>((DependencyObject)result.VisualHit);
          if (item == null)
            return;

          // Find the data behind the ListViewItem
          var contact = lbLocalPhotos.ItemContainerGenerator.ItemFromContainer(item);

          // Initialize the drag & drop operation
          pictureDraggingBelow = contact.ToString();
          Console.WriteLine(pictureDraggingBelow.Substring(pictureDraggingBelow.Length - 10));
          Core.Device.SendMessage(new PhotoMessage() { Type = PhotoMessage.PhotoMessageType.DragStarted });
        }
        return;
      }

      if (e.Action == TouchAction.Up)
      {
        Core.Device.SendMessage(new PhotoMessage() { Type = PhotoMessage.PhotoMessageType.DragFinished });

        String picture = pictureDraggingBelow;
        pictureDraggingBelow = String.Empty;
        startPos = new Point(0, 0);

        Point mousePos = topToBelowMatrix.Transform(new Point(e.Location.X, e.Location.Y));
        if (mousePos.Y < ActualHeight / 2)
          return;

        if (picture != String.Empty)
          SendPicture(Core.Device.DeviceOnTop, picture);
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
        Thread.Sleep(100);
      }
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
        });
    }
  }
}
