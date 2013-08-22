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
using UofM.HCI.tPad.Controls;

namespace UofM.HCI.tPad.App.InfSeeking
{
  /// <summary>
  /// Interaction logic for InfSeekingApp.xaml
  /// </summary>
  public partial class InfSeekingApp : UserControl, ITPadApp, INotifyPropertyChanged
  {

    public enum NotificationType { PasteRequest, DataEmpty, DataError };

    public event BoolEventHandler IsTopApp;
    public event RequestUserFocus RequestFocus;
    public event EventHandler Closed;
    public event PropertyChangedEventHandler PropertyChanged;

    public Guid AppUUID { get; private set; }
    public TPadCore Core { get; set; }
    public ITPadAppContainer Container { get; set; }
    public ITPadAppController Controller { get; set; }

    public Dictionary<String, Object> Context
    {
      get { return null; }
    }

    public event EventHandler SendResultOK;
    public event EventHandler SendErrorData;
    public event Exp1EventHandler GetNextTarget;

    private Exp1Target currentTarget = null;
    public Exp1Target CurrentTarget 
    {
      get { return currentTarget; }
      set 
      {
        currentTarget = value;
        OnPropertyChanged("CurrentTarget");
      }
    }

    private NotificationDialog notification = null;

    public InfSeekingApp(TPadCore core, ITPadAppContainer container, ITPadAppController controller, Guid appUUID)
    {
      Core = core;
      Container = container;
      AppUUID = appUUID;
      InitializeComponent();

      notification = new NotificationDialog(Core, Guid.NewGuid());
      notification.ClickedOK += new EventHandler(notification_ClickedOK);
      notification.ClickedCancel += new EventHandler(notification_ClickedCancel);
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

    public void LoadInitContext(Dictionary<string, Object> context) 
    {
      if (context == null)
        return;
      if (!context.ContainsKey("result"))
        return;
      tbTarget.Text = context["result"] as String;
    }

    private void tbSecond_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
      if (CurrentTarget.Condition.Method == SwitchingMethod.TapNFlip)
        return;

      MessabeBoxShow("Paste from clipboard?", "Yes", "No", NotificationType.PasteRequest);
    }

    private void MessabeBoxShow(String message, String yesButton, String noButton, NotificationType type)
    {
      Dictionary<String, Object> context = new Dictionary<String, Object>();
      context.Add("message", message);
      context.Add("buttonOK", yesButton);
      context.Add("buttonCancel", noButton);
      context.Add("sender", null);
      context.Add("currentApp", null);
      context.Add("state", type);

      notification.LoadInitContext(context);
      Container.LoadTPadApp(notification, true);
    }

    void notification_ClickedOK(object sender, EventArgs e)
    {
      if (((NotificationType)notification.State) != NotificationType.PasteRequest)
        return;

      if (Clipboard.ContainsText())
        tbTarget.Text = Clipboard.GetText();
    }

    void notification_ClickedCancel(object sender, EventArgs e)
    {

    }

    private void tpKeyboard_EnterKeyPressed(object sender, EventArgs e)
    {
      if (focusedTB == null)
        focusedTB = tbTarget;

      focusedTB.Text = tpKeyboard.CurrentText.ToString().Trim();
      tpKeyboard.CurrentText.Clear();
    }

    TextBox focusedTB = null;
    private void tb_GotFocus(object sender, RoutedEventArgs e)
    {
      focusedTB = sender as TextBox;
    }

    private void bSend_Click(object sender, RoutedEventArgs e)
    {
      if (tbTarget.Text == String.Empty)
      {
        MessabeBoxShow("Please enter the requested data", "OK", "Cancel", NotificationType.DataEmpty);
        return;
      }

      int target = Int32.Parse(tbTarget.Text);
      if(target != CurrentTarget.Target)
      {
        SendErrorData(this, null);
        MessabeBoxShow("Please enter the right number", "OK", "Cancel", NotificationType.DataError);
        return;
      }

      SendResultOK(this, null);

      CurrentTarget = GetNextTarget(this, null);
      tbTarget.Text = String.Empty;
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
      if (GetNextTarget != null)
        CurrentTarget = GetNextTarget(this, null);

      if (currentTarget == null)
      {
        MessageBox.Show("No experiment is currently being executed");
        return;
      }
    }
  }
}
