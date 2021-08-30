using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GalaSoft.MvvmLight.Command;

namespace TextEditor_WPF.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private bool popupIsOpen;
        public bool PopupIsOpen
        {
            get { return popupIsOpen; }
            set { popupIsOpen = value; OnPropertyChanged(); }
        }

        private string filePath;
        public string FilePath
        {
            get { return filePath; }
            set { filePath = value; OnPropertyChanged(); }
        }

        private string text;
        public string Text
        {
            get { return text; }
            set { text = value; OnPropertyChanged(); }
        }

        private string wordCounts;
        public string WordsCount
        {
            get { return wordCounts; }
            set { wordCounts = value; OnPropertyChanged(); }
        }

        public string Value { get; set; }

        public Stack<string> UndoStack { get; set; } = new Stack<string>();
        public Stack<string> RedoStack { get; set; } = new Stack<string>();

        public RelayCommand SaveCommand { get; set; }
        public RelayCommand SaveAsCommand { get; set; }
        public RelayCommand<CheckBox> AutoSaveCommand { get; set; }
        public RelayCommand<TextBox> CutCommand { get; set; }
        public RelayCommand<TextBox> CopyCommand { get; set; }
        public RelayCommand<TextBox> PasteCommand { get; set; }
        public RelayCommand<TextBox> SelectAllCommand { get; set; }
        public RelayCommand<TextBox> FontCommand { get; set; }
        public RelayCommand<TextBox> FontColorCommand { get; set; }
        public RelayCommand<TextBox> WordCountCommand { get; set; }
        public RelayCommand<CheckBox> ExitCommand { get; set; }
        public RelayCommand UndoCommand { get; set; }
        public RelayCommand RedoCommand { get; set; }
        public RelayCommand OpenCommand { get; set; }
        public RelayCommand NewWindowCommand { get; set; }

        public MainViewModel()
        {
            WordsCount = "Count of words : 0";

            SaveAsCommand = new RelayCommand(SaveAs);
            SaveCommand = new RelayCommand(Save);
            UndoCommand = new RelayCommand(Undo);
            RedoCommand = new RelayCommand(Redo);
            OpenCommand = new RelayCommand(Open);
            NewWindowCommand = new RelayCommand(NewWindow);
            AutoSaveCommand = new RelayCommand<CheckBox>(AutoSave);
            CutCommand = new RelayCommand<TextBox>(Cut);
            CopyCommand = new RelayCommand<TextBox>(Copy);
            PasteCommand = new RelayCommand<TextBox>(Paste);
            SelectAllCommand = new RelayCommand<TextBox>(SelectAll);
            FontCommand = new RelayCommand<TextBox>(Font);
            FontColorCommand = new RelayCommand<TextBox>(FontColor);
            ExitCommand = new RelayCommand<CheckBox>(Exit);
            WordCountCommand = new RelayCommand<TextBox>(WordCount);
        }

        public void WordCount(TextBox textBox)
        {
            PopupIsOpen = true;
            string input = textBox.Text;
            string pattern = "[^\\w]";
            int count = 0;
            string[] words = Regex.Split(input, pattern, RegexOptions.IgnoreCase);
            for (int i = words.GetLowerBound(0); i <= words.GetUpperBound(0); i++)
            {
                if (words[i].ToString() == string.Empty)
                {
                    count--;
                }
                count++;
            }
            WordsCount = "Count of words : " + count.ToString();
        }

        public void NewWindow()
        {
            _ = Process.Start("TextEditor_WPF.exe");
        }

        public void Open()
        {
            System.Windows.Forms.OpenFileDialog file = new System.Windows.Forms.OpenFileDialog();
            if (file.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Text = File.ReadAllText(file.FileName);
            }
        }

        public void Exit(CheckBox checkBox)
        {
            if (checkBox.IsChecked.Value)
            {
                Save();
            }
            Application.Current.Shutdown();
        }

        public void FontColor(TextBox textBox)
        {
            System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog();
            if (colorDialog.ShowDialog().Equals(System.Windows.Forms.DialogResult.OK))
            {
                System.Drawing.Color color = colorDialog.Color;
                Color converted = Color.FromArgb(color.A, color.R, color.G, color.B);
                textBox.Foreground = new SolidColorBrush(converted);
            }
        }

        public void Font(TextBox textBox)
        {
            System.Windows.Forms.FontDialog fontDialog = new System.Windows.Forms.FontDialog();
            if (fontDialog.ShowDialog().Equals(System.Windows.Forms.DialogResult.OK))
            {
                textBox.FontFamily = new System.Windows.Media.FontFamily(fontDialog.Font.Name);
                textBox.FontSize = fontDialog.Font.Size * 98.0 / 72.0;
                textBox.FontWeight = fontDialog.Font.Bold ? FontWeights.Bold : FontWeights.Regular;
                textBox.FontStyle = fontDialog.Font.Italic ? FontStyles.Italic : FontStyles.Normal;
            }
        }

        public void SelectAll(TextBox textBox)
        {
            textBox.SelectAll();
        }

        public void Paste(TextBox textBox)
        {
            if (Value != null)
            {
                textBox.Text = textBox.Text.Insert(textBox.SelectionStart, Value);
                textBox.SelectionStart += Value.Length;
            }
        }

        public void Copy(TextBox textBox)
        {
            Value = textBox.SelectedText;
        }

        public void Cut(TextBox textBox)
        {
            Value = textBox.SelectedText;
            textBox.Text = textBox.Text.Remove(textBox.SelectionStart, textBox.SelectionLength);
        }

        public void AutoSave(CheckBox checkBox)
        {
            checkBox.ToolTip = checkBox.IsChecked.Value ? "Autosave : ON" : "Autosave : OFF";
        }

        public void Save()
        {
            if (FilePath == null)
            {
                Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Text file (*.txt)|*.txt"
                };
                if (saveFileDialog.ShowDialog() == true)
                {
                    File.WriteAllText(saveFileDialog.FileName, Text);
                    FilePath = saveFileDialog.FileName;
                }
            }
            else
            {
                File.WriteAllText(FilePath, Text);
            }
            UndoStack.Push(Text);
        }

        public void SaveAs()
        {
            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Text file (*.txt)|*.txt"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllText(saveFileDialog.FileName, Text);
                FilePath = saveFileDialog.FileName;
            }
        }

        public void Undo()
        {
            if (UndoStack.Count() != 0)
            {
                string undo = UndoStack.Pop();
                RedoStack.Push(undo);
                Text = UndoStack.First();
            }
        }

        public void Redo()
        {
            if (RedoStack.Count() == 0)
            {
                Text = UndoStack.First();
                return;
            }
            string redo = RedoStack.Pop();
            UndoStack.Push(redo);
            Text = UndoStack.First();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
