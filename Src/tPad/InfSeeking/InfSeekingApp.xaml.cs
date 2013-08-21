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

namespace UofM.HCI.tPad.App.InfSeeking
{
  /// <summary>
  /// Interaction logic for InfSeekingApp.xaml
  /// </summary>
  public partial class InfSeekingApp : UserControl, ITPadApp, INotifyPropertyChanged
  {

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
    public event EventHandler SendErrorResult;
    public event Exp1EventHandler GetNextPair;

    private Exp1Pair currentPair = null;
    public Exp1Pair CurrentPair 
    {
      get { return currentPair; }
      set 
      {
        currentPair = value;
        OnPropertyChanged("CurrentPair");
      }
    }

    public InfSeekingApp(TPadCore core, ITPadAppContainer container, ITPadAppController controller, Guid appUUID)
    {
      AppUUID = appUUID;
      InitializeComponent();
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
      tbSecond.Text = context["result"] as String;
    }

    private void tbSecond_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
      if (MessageBox.Show("Paste contents", "Copy+Paste", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
      {
        if (Clipboard.ContainsText())
          tbSecond.Text = Clipboard.GetText();
      }
    }

    private void tpKeyboard_EnterKeyPressed(object sender, EventArgs e)
    {
      if (focusedTB == null)
        focusedTB = tbSecond;

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
      if (tbSecond.Text == String.Empty || tbResult.Text == String.Empty)
      {
        MessageBox.Show("Please enter both the requested data and the result");
        return;
      }

      int factor2 = Int32.Parse(tbSecond.Text);
      if(factor2 != CurrentPair.Factor2)
      {
        SendErrorData(this, null);
        MessageBox.Show("Please enter the right second factor");
        return;
      }

      int result = Int32.Parse(tbResult.Text);
      if (result != (CurrentPair.Factor1 + CurrentPair.Factor2))
      {
        SendErrorResult(this, null);
        MessageBox.Show("Please enter the right operaiton result");
        return;
      }

      SendResultOK(this, null);

      CurrentPair = GetNextPair(this, null);
      tbResult.Text = String.Empty;
      tbSecond.Text = String.Empty;
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
      if (GetNextPair != null)
        CurrentPair = GetNextPair(this, null);

      if (currentPair == null)
      {
        MessageBox.Show("No experiment is currently being executed");
        return;
      }
    }
  }
}
