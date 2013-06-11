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
using System.Text.RegularExpressions;

namespace UofM.HCI.tPad
{
  /// <summary>
  /// Interaction logic for UserControlKeyboard.xaml
  /// </summary>
  public partial class TPadKeyboard : UserControl, INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;
    public void OnPropertyChanged(String name)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(name));
    }

    public event EventHandler EnterKeyPressed;
    private void OnEnterKeyPressed()
    {
      if (EnterKeyPressed != null)
        EnterKeyPressed(this, null);
    }

    public event EventHandler AlphaNumericKeyPressed;
    private void OnAlphaNumericKeyPressed()
    {
      if (AlphaNumericKeyPressed != null)
        AlphaNumericKeyPressed(this, null);
    }
        
    private bool showNumericKeyboard;
    public bool ShowNumericKeyboard
    {
      get { return showNumericKeyboard; }
      set
      {
        showNumericKeyboard = value;
        OnPropertyChanged("ShowNumericKeyboard");
      }
    }
    
    public string ResultString
    {
      get { return currentTextLine.ToString(); }
    }

    private StringBuilder currentTextLine = new StringBuilder();
    public StringBuilder CurrentTextLine
    {
      get { return currentTextLine; }
      private set
      {
        currentTextLine = value;
        OnPropertyChanged("CurrentTextLine");
        OnPropertyChanged("ResultString");
      }
    }

    private StringBuilder currentText = new StringBuilder();
    public StringBuilder CurrentText
    {
      get { return currentText; }
      private set
      {
        currentText = value;
        OnPropertyChanged("CurrentText");
        OnPropertyChanged("ResultString");
      }
    }

    public void ResultClear()
    {
      CurrentText = new StringBuilder();
      CurrentTextLine = new StringBuilder();
    }

    private float keyboardWidth = 300;
    public float KeyboardWidth
    {
      get { return keyboardWidth; }
      set
      {
        keyboardWidth = value;
        this.OnPropertyChanged("KeyboardWidth");
      }
    }

    public TPadKeyboard()
    {
      InitializeComponent();
      CurrentText = new StringBuilder();
      CurrentTextLine = new StringBuilder();
    }

    private void keyboardButton_Click(object sender, RoutedEventArgs e)
    {
      Button button = sender as Button;
      //IsEnterPressed = false;

      if (button != null)
      {        
        switch (button.CommandParameter.ToString())
        {
          case "LSHIFT":
            Regex upperCaseRegex = new Regex("[A-Z]");
            Regex lowerCaseRegex = new Regex("[a-z]");
            Button btn;

            foreach (UIElement elem in AlfaKeyboard.Children) //iterate the main grid
            {
              Grid grid = elem as Grid;
              if (grid != null)
              {
                foreach (UIElement uiElement in grid.Children)  //iterate the single rows
                {
                  btn = uiElement as Button;
                  if (btn != null) // if button contains only 1 character
                  {
                    if (btn.Content.ToString().Length == 1)
                    {
                      if (upperCaseRegex.Match(btn.Content.ToString()).Success) // if the char is a letter and uppercase
                        btn.Content = btn.Content.ToString().ToLower();
                      else if (lowerCaseRegex.Match(button.Content.ToString()).Success) // if the char is a letter and lower case
                        btn.Content = btn.Content.ToString().ToUpper();
                    }
                  }
                }
              }
            }
            break;

          case "ALT":
          case "CTRL":
            break;

          case "RETURN":
            //IsEnterPressed = true;
            OnEnterKeyPressed();
            CurrentTextLine = new StringBuilder();
            CurrentText.Append("\r\n");
            break;

          case "BACK":
            if (CurrentText.Length > 0)
            {
              CurrentText.Remove(CurrentText.Length - 1, 1);
          

              OnAlphaNumericKeyPressed();
            }
            if (CurrentTextLine.Length > 0)
            {
              CurrentTextLine.Remove(CurrentTextLine.Length - 1, 1);
            }
            OnPropertyChanged("ResultString");
            break;

          default:
            CurrentText.Append(button.Content.ToString());
            CurrentTextLine.Append(button.Content.ToString());
            OnPropertyChanged("ResultString");

            OnAlphaNumericKeyPressed();  
            
            break;
        }
      }
    }
  }
}
