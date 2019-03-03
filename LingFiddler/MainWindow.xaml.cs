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

namespace Lx
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

        protected Lexicon localLexicon;
        public Lexicon LocalLexicon
        {
            get
            {
                if (localLexicon == null)
                    localLexicon = new Lexicon();

                return localLexicon;
            }

            set
            {
                localLexicon = value;
            }
        }

        public List<Morph> SelectedWords { get; set; }
        public List<Morph> WordList
        {
            get
            {
                return LocalLexicon.Keys.OrderBy(w => w.Graph).ToList();
            }
        }

        public Dictionary<Ngram, int> Bigrams;
        public Dictionary<Ngram, int> Trigrams;

        public class NgramView
        {
            public string Onset { get; set; }
            public string Coda { get; set; }
            public string Value { get; set; }
            public int Weight { get; set; }

            public static List<NgramView> GetViewList(Dictionary<Ngram, int> ngrams)
            {
                var outList = new List<NgramView>();

                foreach (var ngram in ngrams.Keys)
                {
                    outList.Add(
                        new NgramView()
                        {
                            Onset = ngram.Onset,
                            Coda = ngram.Coda,
                            Value = ngram.Value,
                            Weight = ngrams[ngram]
                        }
                        );
                }

                return outList;
            }
        }

        public List<NgramView> BigramView;
        public List<NgramView> TrigramView;

        public MainWindow()
        {
            InitializeComponent();
            PathBox.Text = @"C:\Users\arcan\Documents\Linguistics\Jules Verne_Le Chateau des Carpathes.txt";
            Bigrams = new Dictionary<Ngram, int>();
            Trigrams = new Dictionary<Ngram, int>();
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
            LocalLexicon.Clear();

            foreach (System.Text.RegularExpressions.Match m in CurrentWordPattern.Matches(TextBlock.Text))
            {
                LocalLexicon.Add(m.Value);

                foreach (var bigram in Ngram.Parse(m.Value, 2))
                {
                    var key = Bigrams.Keys.FirstOrDefault(k => k.Value == bigram.Value);

                    if (key != null)
                    {
                        Bigrams[key] += 1;
                    }
                    else
                    {
                        Bigrams.Add(bigram, 1);
                    }
                }
                
                foreach (var trigram in Ngram.Parse(m.Value, 3))
                {
                    var key = Trigrams.Keys.FirstOrDefault(k => k.Value == trigram.Value);

                    if (key != null)
                    {
                        Trigrams[key] += 1;
                    }
                    else
                    {
                        Trigrams.Add(trigram, 1);
                    }
                }
                
            }

            UpdateWordGrid();
        }

        private void UpdateWordGrid()
        {
            WordGrid.ItemsSource = null;
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

        private void UpdateNgramGrid(Dictionary<Ngram, int> source, List<NgramView> target)
        {
            target = new List<NgramView>(NgramView.GetViewList(source));

            WordGrid.ItemsSource = null;
            WordGrid.Columns.Clear();

            DataGridTextColumn onsetColumn = new DataGridTextColumn()
            {
                Header = "Onset",
                Binding = new Binding("Onset"),
                Width = 50
            };

            DataGridTextColumn codaColumn = new DataGridTextColumn()
            {
                Header = "Coda",
                Binding = new Binding("Coda"),
                Width = 50
            };

            DataGridTextColumn valueColumn = new DataGridTextColumn()
            {
                Header = "Value",
                Binding = new Binding("Value"),
                Width = 50
            };

            DataGridTextColumn weightColumn = new DataGridTextColumn()
            {
                Header = "Weight",
                Binding = new Binding("Weight"),
                Width = 50
            };

            WordGrid.Columns.Add(onsetColumn);
            WordGrid.Columns.Add(codaColumn);
            WordGrid.Columns.Add(valueColumn);
            WordGrid.Columns.Add(weightColumn);

            WordGrid.ItemsSource = target;
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

            SelectedWords.ForEach(w => LocalLexicon.Remove(w));
            UpdateWordGrid();
        }

        private void SaveWords_Click(object sender, RoutedEventArgs e)
        {
            //Add LocalLexicon to the CurrentLanguage.Lexicon
        }

        protected void MyTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // When the RichTextBox loses focus the user can no longer see the selection.
            // This is a hack to make the RichTextBox think it did not lose focus.
            e.Handled = true;
        }

        private void Words_Click(object sender, RoutedEventArgs e)
        {
            UpdateWordGrid();
        }

        private void Bigrams_Click(object sender, RoutedEventArgs e)
        {
            UpdateNgramGrid(Bigrams, BigramView);
        }

        private void Trigrams_Click(object sender, RoutedEventArgs e)
        {
            UpdateNgramGrid(Trigrams, TrigramView);
        }

        private void CreateWords_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
