using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

namespace LingFiddler
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string dbSource = "language.db";//"Data Source=database.db;Version=3;New=True;Compress=True;";
        public string ConnectionString
        {
            get
            {
                return "Data Source=" + dbSource + ";Version=3;";
            }
        }

        public string CurrentText { get; set; }
        public int CurrentTextLineCount
        {
            get { return CurrentText.ToCharArray().Count(c => c == '\n') + 1; }
        }

        public int CurrentWordCount { get; set; }
        public HashSet<char> CurrentCharSet { get; set; }

        private System.Text.RegularExpressions.Regex currentWordPattern;
        public System.Text.RegularExpressions.Regex CurrentWordPattern
        {
            get
            {
                if (currentWordPattern == null)
                    currentWordPattern = new System.Text.RegularExpressions.Regex(WordPattern.Text);

                return currentWordPattern;
            }
        }

        public List<Morph> SelectedWords { get; set; }
        public List<Morph> WordList
        {
            get { return Morph.Words.OrderBy(w => w.Graph).ToList(); }
        }

        public MainWindow()
        {
            InitializeComponent();
            PathBox.Text = @"C:\Users\arcan\Documents\Linguistics\Jules Verne_Le Chateau des Carpathes.txt";
            //FilterChars.Text = new string (Morph.FilterChars);
        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            string path = PathBox.Text;

            if (File.Exists(path))
            {
                TextBlock.Text = File.ReadAllText(path, Encoding.UTF8);
            }
        }

        private void CurrentTextUpdated(object sender, TextChangedEventArgs e)
        {
            CurrentText = TextBlock.Text;

            var textChars = TextBlock.Text.ToCharArray();
            CharCount.Text = textChars.Length.ToString();
            CurrentCharSet = new HashSet<char>(textChars.Distinct());
            UniqueCharCount.Text = CurrentCharSet.Count().ToString();

            UpdateWordCount();
        }

        private void UpdateCurrentWordPattern(object sender, TextChangedEventArgs e)
        {
            currentWordPattern = new System.Text.RegularExpressions.Regex(WordPattern.Text);
            UpdateWordCount();
        }

        private void UpdateWordCount()
        {
            CurrentWordCount = String.IsNullOrEmpty(CurrentText) ? 0 : CurrentWordPattern.Matches(CurrentText).Count;
            WordCount.Text = CurrentWordCount.ToString();
        }

        private SQLiteConnection CreateConnection()
        {
            var connection = new SQLiteConnection(ConnectionString);

            try
            {
                connection.Open();
            }
            catch (Exception ex)
            {
                //put some error out here, I think
            }

            return connection;
        }

        private void CreateTable(SQLiteConnection connection)
        {
            SQLiteCommand command = connection.CreateCommand();

            string CreateWordTable =
                "CREATE TABLE Words" +
                "(Word VARCHAR(100))";

            command.CommandText = CreateWordTable;
            command.ExecuteNonQuery();
        }

        private void GetWords_Click(object sender, RoutedEventArgs e)
        {
            Morph.Clear();
            //Morph.FilterChars = FilterChars.Text.ToCharArray();

            foreach (System.Text.RegularExpressions.Match m in CurrentWordPattern.Matches(TextBlock.Text))
            {
                Morph.Add(m.Value);
            }

            UpdateWordGrid();
        }

        private void UpdateWordGrid()
        {
            WordGrid.Columns.Clear();

            DataGridTextColumn wordColumn = new DataGridTextColumn()
            {
                Header = "Graph",
                Binding = new Binding("Graph"),
                Width = 120
            };

            DataGridTextColumn lengthColumn = new DataGridTextColumn()
            {
                Header = "Length",
                Binding = new Binding("Length"),
                Width = 50
            };

            DataGridTextColumn freqColumn = new DataGridTextColumn()
            {
                Header = "Frequency",
                Binding = new Binding("Frequency"),
                Width = 50

            };

            WordGrid.Columns.Add(wordColumn);
            WordGrid.Columns.Add(lengthColumn);
            WordGrid.Columns.Add(freqColumn);

            WordGrid.ItemsSource = WordList;
        }

        private void WordGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedWords == null)
                SelectedWords = new List<Morph>();
            else
                SelectedWords.Clear();

            foreach(Morph w in e.AddedItems)
            {
                SelectedWords.Add(w);
            }            
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedWords == null)
                SelectedWords = new List<Morph>();

            SelectedWords.ForEach(w => Morph.Remove(w));
            UpdateWordGrid();
        }

        private void SaveWords_Click(object sender, RoutedEventArgs e)
        {
            //Here's where we write the words to the DB
        }

        protected void MyTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // When the RichTextBox loses focus the user can no longer see the selection.
            // This is a hack to make the RichTextBox think it did not lose focus.
            e.Handled = true;
        }
    }
}
