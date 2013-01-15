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

namespace UofM.HCI.tPab
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
       
    private string result = String.Empty;
    public string Result
    {
      get { return result; }
      private set
      {
        result = value;
        OnPropertyChanged("Result");
      }
    }
    public void ResultClear()
    {
      Result = "";
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
      Result = "";
    }

    private void keyboardButton_Click(object sender, RoutedEventArgs e)
    {
      Button button = sender as Button;
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
            //stickyNote.Text += Result.ToString();
            currentNote.Text += "\r\n";
            Result = "";
            break;

          case "BACK":
            if (Result.Length > 0 && currentNote.Text.Length > 0)
            {
              Result = Result.Remove(Result.Length - 1);
              currentNote.Text = currentNote.Text.Remove(currentNote.Text.Length - 1);
            }
            break;

          default:
            Result += button.Content.ToString();
            currentNote.Text += button.Content.ToString();
            break;
        }
      }
    }

    private TextBox currentNote;
    public void setCurrentNote(TextBox note)
    {
      currentNote = note;
    }
    public TextBox getCurrentNote()
    {
      return currentNote;
    }
  }
}
