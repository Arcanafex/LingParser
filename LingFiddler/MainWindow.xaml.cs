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

        public List<Word> SelectedWords { get; set; }
        public List<Word> WordList
        {
            get { return Word.Words.OrderBy(w => w.Graph).ToList(); }
            set { Word.Words = value; }
        }

        public MainWindow()
        {
            InitializeComponent();
            PathBox.Text = @"C:\Users\arcan\Documents\Linguistics\Jules Verne_Le Chateau des Carpathes.txt";
        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            string path = PathBox.Text;

            if (File.Exists(path))
            {
                TextBlock.Text = File.ReadAllText(path, Encoding.UTF8);
            }

            UpdateCurrentText();
        }

        private void UpdateCurrentText()
        {
            CurrentText = TextBlock.Text;
            LineCount.Text = CurrentTextLineCount.ToString();
            CharCount.Text = TextBlock.Text.ToCharArray().Length.ToString();
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
            UpdateCurrentText();

            var pattern = new System.Text.RegularExpressions.Regex(WordPattern.Text);

            Word.Clear();

            foreach (System.Text.RegularExpressions.Match m in pattern.Matches(TextBlock.Text))
            {
                Word.Add(m.Value);
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

        protected void MyTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // When the RichTextBox loses focus the user can no longer see the selection.
            // This is a hack to make the RichTextBox think it did not lose focus.
            e.Handled = true;
        }
    }
}
