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

        public List<Word> SelectedWords { get; set; }
        public List<Word> WordList
        {
            get { return Word.Words.OrderBy(w => w.Graph).ToList(); }
        }

        public MainWindow()
        {
            InitializeComponent();
            PathBox.Text = @"C:\Users\arcan\Documents\Linguistics\Jules Verne_Le Chateau des Carpathes.txt";
            FilterChars.Text = new string (Word.FilterChars);
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
            Grapheme.Graphemes = new HashSet<char>(textChars.Distinct());
            UniqueCharCount.Text = Grapheme.Graphemes.Count().ToString();

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
            Word.Clear();
            Word.FilterChars = FilterChars.Text.ToCharArray();

            float weight = 1 / (CurrentWordCount == 0 ? 1 : CurrentWordCount);

            foreach (System.Text.RegularExpressions.Match m in CurrentWordPattern.Matches(TextBlock.Text))
            {
                Word.Add(m.Value, weight);
            }

            UpdateWordGrid();
        }

        private void UpdateWordGrid()
        {
            WordGrid.ItemsSource = WordList;
        }

        private void WordGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedWords == null)
                SelectedWords = new List<Word>();
            else
                SelectedWords.Clear();

            foreach(Word w in e.AddedItems)
            {
                SelectedWords.Add(w);
            }            
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedWords == null)
                SelectedWords = new List<Word>();

            SelectedWords.ForEach(w => Word.Remove(w));
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
