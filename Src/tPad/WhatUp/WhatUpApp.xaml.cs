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
using Ubicomp.Utils.NET.MTF;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using Microsoft.Win32;

namespace UofM.HCI.tPad.App.WhatUp
{
  /// <summary>
  /// Interaction logic for UserControl1.xaml
  /// </summary>
  public partial class WhatUpApp : UserControl, ITPadApp, INotifyPropertyChanged, ITransportListener
  {

    public event EventHandler Closed;
    public event PropertyChangedEventHandler PropertyChanged;
    public event BoolEventHandler IsTopApp;
    public event RequestUserFocus RequestFocus;

    public Guid AppUUID { get; private set; }
    public TPadCore Core { get; set; }
    public ITPadAppContainer Container { get; set; }
    public ITPadAppController Controller { get; set; }
    public Dictionary<String, Object> Context { get { return null; } }

    public ObservableCollection<WhatUpMessage> Messages { get; set; }

    public WhatUpApp(TPadCore core, ITPadAppContainer container, ITPadAppController controller, Guid appUUID)
    {
      AppUUID = appUUID;
      Core = core;
      Container = container;
      Controller = controller;

      TransportComponent.Instance.TransportListeners.Add(this);
      Messages = new ObservableCollection<WhatUpMessage>();

      InitializeComponent();
    }

    public void DeActivate() { }

    public void Close()
    {
      if (TransportComponent.Instance.TransportListeners.Contains(this))
        TransportComponent.Instance.TransportListeners.Remove(this);

      if (Closed != null)
        Closed(this, EventArgs.Empty);
    }

    public void OnPropertyChanged(String name)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(name));
    }

    public void LoadInitContext(Dictionary<string, Object> init) 
    {
      if (init == null)
        return;

      if (init.ContainsKey("image"))
      {
        Messages.Add(new WhatUpMessage()
        {
          From = String.Format("Device-{0}", Core.Device.ID),
          Attachment = init["image"]
        });
      }
    }

    public int MessageType
    {
      get { return WhatUpMessage.MessageID; }
    }

    public void MessageReceived(TransportMessage message, string rawMessage)
    {
      if (message.MessageSource.ResourceId == Core.Device.TMessageEventSource.ResourceId)
        return;
      if (message.MessageData == null || !(message.MessageData is WhatUpMessage))
        return;

      Dispatcher.Invoke(DispatcherPriority.Render,
        (Action)delegate()
        {
          WhatUpMessage wuMessage = message.MessageData as WhatUpMessage;
          Messages.Add(wuMessage);

          if (IsTopApp(this, null))
            return;

          RequestFocus(this, String.Format("{0}: {1}", wuMessage.From, wuMessage.Message), "Reply", "Cancel");
        });
    }

    private void tpKeyboard_EnterKeyPressed(object sender, EventArgs e)
    {
      String currentText = tpKeyboard.CurrentText.ToString().Trim();
      tpKeyboard.CurrentText.Clear();

      if (currentText == String.Empty)
        return;

      var messageToSend = new WhatUpMessage()
      {
        From = String.Format("Device-{0}", Core.Device.ID),
        Message = currentText
      };

      Messages.Add(messageToSend);
      Core.Device.SendMessage(messageToSend);
    }

    private void bAttach_Click(object sender, RoutedEventArgs e)
    {
      OpenFileDialog openFileDialog1 = new OpenFileDialog();

      String path = String.Format("{0}\\Device{1}\\Photos\\", Environment.CurrentDirectory, Core.Device.ID);
      openFileDialog1.InitialDirectory = path;
      openFileDialog1.Filter = "image files (*.jpg)|*.jpg";
      openFileDialog1.FilterIndex = 2;
      openFileDialog1.RestoreDirectory = true;
      openFileDialog1.Multiselect = false;

      if (openFileDialog1.ShowDialog() == true)
      {
        try
        {
          String file = openFileDialog1.FileName;
          Messages.Add(new WhatUpMessage()
          {
            From = String.Format("Device-{0}", Core.Device.ID),
            Attachment = file
          });
        }
        catch (Exception ex)
        {
          MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
        }
      }
    }
  }
}
