using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    public partial class MainWindow : Window
    {
        public string CurrentText { get; set; }

        private Regex currentLinePattern;
        public Regex CurrentLinePattern
        {
            get
            {
                if (currentLinePattern == null)
                    currentLinePattern = new Regex(LinePattern.Text, RegexOptions.Singleline);

                return currentLinePattern;
            }
        }

        private Regex currentWordPattern;
        public Regex CurrentWordPattern
        {
            get
            {
                if (currentWordPattern == null)
                    currentWordPattern = new Regex(WordPattern.Text);

                return currentWordPattern;
            }
        }

        internal int currentTextLineCount = 0;
        public string CurrentTextLineCount
        {
            get { return currentTextLineCount.ToString(); }
        }
        public int CurrentWordCount { get; set; }
        public HashSet<char> CurrentCharSet { get; set; }
        public string UniqueWordCount
        {
            get
            {
                if (LocalText != null && LocalText.Lexicon != null)
                {
                    return LocalText.Lexicon.Keys.Count.ToString();
                }
                else
                {
                    return "0";
                }
            }
        }

        internal int currentNgramSize = 2;
        internal int currentGenerateWordsSize = 10;
        internal int currentMarkovChainSize = 2;
        internal int currentGenerateLinesSize = 5;

        internal enum ViewMode { Text, Lines, Generated }
        internal ViewMode currentViewMode = ViewMode.Text;

        internal enum GridMode { Words, Ngrams, Generated }
        internal GridMode currentGridMode = GridMode.Words;

        #region Local Objects
        protected Text localText;
        public Text LocalText
        {
            get
            {
                if (localText == null)
                    localText = new Text();

                return localText;
            }

            set
            {
                localText = value;
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

        public List<Morph> WordList
        {
            get
            {
                return LocalLexicon.Keys.OrderBy(w => w.Graph).ToList();
            } 
        }
        public List<Morph> SelectedWords { get; set; }

        public Dictionary<string, int> localNgrams;
        public FiniteStateAutomoton<char> localNgramFSA;

        public Dictionary<string, int> localHMM;
        public FiniteStateAutomoton<string> localHMMFSA;

        #endregion

        public Text CreatedText { get; set; }

        public Lexicon CreatedLexicon { get; set; }
        public List<Morph> CreatedWordList
        {
            get
            {
                if (CreatedLexicon == null)
                    CreatedLexicon = new Lexicon();

                return CreatedLexicon.Keys.ToList();
            }
        }


        public class NgramView
        {
            public string Onset { get; set; }
            public string Coda { get; set; }
            public string Value { get; set; }
            public int Weight { get; set; }

            public static List<NgramView> GetViewList(Dictionary<string, int> ngrams)
            {
                if (ngrams == null)
                    return null;

                var outList = new List<NgramView>();

                foreach (var ngram in ngrams.Keys)
                {
                    outList.Add(
                        new NgramView()
                        {
                            Onset = ngram.Substring(0, 1),
                            Coda = ngram.Substring(ngram.Length - 1),
                            Value = ngram,
                            Weight = ngrams[ngram]
                        }
                    );
                }

                return outList;
            }
        }

        public List<NgramView> NgramViewList;


        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;

            PathBox.Text = @"C:\Users\arcan\Documents\Linguistics\Jules Verne_Le Chateau des Carpathes.txt";

            SizeNgram.Text = currentNgramSize.ToString();
            SizeGenerateWords.Text = currentGenerateWordsSize.ToString();
            SizeMarkovChain.Text = currentMarkovChainSize.ToString();
            SizeGenerateLines.Text = currentGenerateLinesSize.ToString();
        }

        private void LoadText_Click(object sender, RoutedEventArgs e)
        {
            string path = PathBox.Text;
            string loadedText = string.Empty;

            if (File.Exists(path))
            {
                loadedText = File.ReadAllText(path, Encoding.UTF8);
            }

            UpdateTextView(loadedText, ViewMode.Text);
        }

        #region SQLite
        public string dbSource = "language.db";//"Data Source=database.db;Version=3;New=True;Compress=True;";
        public string ConnectionString
        {
            get
            {
                return "Data Source=" + dbSource + ";Version=3;";
            }
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

        private void SaveWords_Click(object sender, RoutedEventArgs e)
        {
            //Add LocalLexicon to the CurrentLanguage.Lexicon
        }

        #endregion

        #region Update_Events

        private void CurrentText_Updated(object sender, TextChangedEventArgs e)
        {
            if (currentViewMode == ViewMode.Text)
            {
                CurrentText = TextBlock.Text;

                var textChars = TextBlock.Text.ToCharArray();
                CountChars.Text = textChars.Length.ToString();
                CurrentCharSet = new HashSet<char>(textChars.Distinct());
                CountUniqueChars.Text = CurrentCharSet.Count().ToString();

                UpdateWordCount();
            }
        }

        private void CurrentWordPattern_Updated(object sender, TextChangedEventArgs e)
        {
            try
            {
                currentWordPattern = new Regex(WordPattern.Text);
                UpdateWordCount();
            }
            catch (Exception ex)
            {
            }
        }

        private void CurrentLinePattern_Updated(object sender, TextChangedEventArgs e)
        {
            try
            {
                currentLinePattern = new Regex(LinePattern.Text, RegexOptions.Singleline);
                UpdateWordCount();
            }
            catch(Exception ex)
            {
            }
        }

        private void CurrentPuncuationPattern_Updated(object sender, TextChangedEventArgs e)
        {
        }

        private void NgramSize_Updated(object sender, TextChangedEventArgs e)
        {
            int n = currentNgramSize;

            if (!int.TryParse(SizeNgram.Text, out n))
            {
                // Return an error message about validation
                SizeNgram.Text = currentNgramSize.ToString();
            }
            else
            {
                if (currentNgramSize != n)
                    currentNgramSize = n;
            }
        }

        private void MarkovChainSize_Updated(object sender, TextChangedEventArgs e)
        {
            int n = currentMarkovChainSize;

            if (!int.TryParse(SizeNgram.Text, out n))
            {
                // Return an error message about validation
                SizeMarkovChain.Text = currentMarkovChainSize.ToString();
            }
            else
            {
                if (currentMarkovChainSize != n)
                    currentMarkovChainSize = n;
            }
        }

        private void GenerateWordsSize_Updated(object sender, TextChangedEventArgs e)
        {
            int n = currentGenerateWordsSize;

            if (!int.TryParse(SizeGenerateWords.Text, out n))
            {
                // Return an error message about validation
                SizeGenerateWords.Text = currentGenerateWordsSize.ToString();
            }
            else
            {
                if (currentGenerateWordsSize != n)
                    currentGenerateWordsSize = n;
            }
        }

        private void GenerateLinesSize_Updated(object sender, TextChangedEventArgs e)
        {
            int n = currentGenerateLinesSize;

            if (!int.TryParse(SizeGenerateLines.Text, out n))
            {
                // Return an error message about validation
                SizeGenerateLines.Text = currentGenerateLinesSize.ToString();
            }
            else
            {
                if (currentGenerateLinesSize != n)
                    currentGenerateLinesSize = n;
            }
        }


        #endregion

        #region Update

        private void UpdateTextView(string text, ViewMode mode)
        {
            currentViewMode = mode;
            TextBlock.Text = text;
        }

        private void UpdateWordCount()
        {
            CurrentWordCount = CurrentWordPattern.Matches(CurrentText).Count;
            currentTextLineCount = CurrentLinePattern.Matches(CurrentText).Count;

            CountWords.Text = CurrentWordCount.ToString();
            CountUniqueWords.Text = UniqueWordCount;
            CountLines.Text = CurrentTextLineCount;
        }

        private void UpdateLocalLexicon()
        {
            foreach (var m in LocalText.Lexicon.Keys)
            {
                LocalLexicon.Add(m.Graph.ToLower(), LocalText.Lexicon[m]);
            }
        }

        private void UpdateWordGrid()
        {
            currentGridMode = GridMode.Words;
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

        private void UpdateNgramGrid(Dictionary<string, int> source, List<NgramView> target)
        {
            currentGridMode = GridMode.Ngrams;
            target = NgramView.GetViewList(source);

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

        private void UpdateGeneratedWordGrid()
        {
            currentGridMode = GridMode.Generated;
            WordGrid.ItemsSource = null;
            WordGrid.Columns.Clear();

            DataGridTextColumn wordColumn = new DataGridTextColumn()
            {
                Header = "Graph",
                Binding = new Binding("Graph"),
                Width = 250
            };

            WordGrid.Columns.Add(wordColumn);
            WordGrid.ItemsSource = CreatedWordList;
        }

        #endregion

        private void WordGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedWords == null)
                SelectedWords = new List<Morph>();
            else
                SelectedWords.Clear();

            foreach (Morph w in e.AddedItems)
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

        protected void MyTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // When the RichTextBox loses focus the user can no longer see the selection.
            // This is a hack to make the RichTextBox think it did not lose focus.
            e.Handled = true;
        }

        #region Show

        private void ShowLines_Click(object sender, RoutedEventArgs e)
        {
            UpdateTextView(LocalText.ToString(), ViewMode.Lines);
        }

        private void ShowText_Click(object sender, RoutedEventArgs e)
        {
            UpdateTextView(CurrentText, ViewMode.Text);
        }

        private void ShowGeneratedLines_Click(object sender, RoutedEventArgs e)
        {
            //UpdateTextView("Nothing to see here yet.", ViewMode.Generated);
        }

        private void ShowWords_Click(object sender, RoutedEventArgs e)
        {
            UpdateWordGrid();
        }

        private void ShowNgrams_Click(object sender, RoutedEventArgs e)
        {
            UpdateNgramGrid(localNgrams, NgramViewList);
        }

        private void ShowCreatedWords_Click(object sender, RoutedEventArgs e)
        {
            UpdateGeneratedWordGrid();
        }

        #endregion

        #region Parse

        private void ParseText_Click(object sender, RoutedEventArgs e)
        {
            ParseCurrentText();
            UpdateWordGrid();
            //UpdateTextView(LocalText.ToString(), ViewMode.Lines);
        }

        private void ParseNgrams_Click(object sender, RoutedEventArgs e)
        {
            ParseLexiconNgramFSA(LocalLexicon);
            UpdateNgramGrid(localNgrams, NgramViewList);
        }

        private void ParseLines_Click(object sender, RoutedEventArgs e)
        {
            ParseTextHMM(LocalText);
        }

        private void ParseCurrentText()
        {
            LocalLexicon.Clear();
            var expElementPattern = new Regex($"({PunctuationPattern.Text})|({WordPattern.Text})");
            var whiteSpacePattern = new Regex(@"[\s\n\r]+", RegexOptions.Singleline | RegexOptions.Multiline);

            // TODO: handling of paragraph breaks and section headers, etc

            foreach (Match l in CurrentLinePattern.Matches(TextBlock.Text))
            {
                //store line, section up into words and punctuation
                string cleanedLine = l.Value.Trim();
                cleanedLine = whiteSpacePattern.Replace(cleanedLine, " ");

                var thisExpression = new Expression(cleanedLine);

                //([\p{P})+|([\w-[_]])+

                foreach (Match m in expElementPattern.Matches(thisExpression.Graph))
                {
                    if (m.Groups.Count > 0)
                    {
                        if (string.IsNullOrEmpty(m.Groups[1].Value))
                        {
                            thisExpression.Add(LocalText.Lexicon.Add(m.Groups[2].Value));
                        }
                        else
                        {
                            thisExpression.Add(LocalText.Morphosyntax.Add(m.Groups[1].Value));
                        }
                    }

                }

                LocalText.Add(thisExpression);
            }

            UpdateLocalLexicon();
            CountUniqueWords.Text = UniqueWordCount;
        }

        private void ParseLexiconNgramFSA(Lexicon lexicon)
        {
            // TODO: give some sort of admonishment if lexicon is empty

            if (localNgrams == null)
                localNgrams = new Dictionary<string, int>();
            else
                localNgrams.Clear();

            localNgramFSA = new FiniteStateAutomoton<char>();

            foreach (var lex in lexicon.Keys)
            {
                foreach (var ngram in Ngram.Parse(lex.Graph, currentNgramSize))
                {
                    if (localNgrams.ContainsKey(ngram))
                    {
                        localNgrams[ngram] += 1;
                    }
                    else
                    {
                        localNgrams.Add(ngram, 1);
                    }
                }

                localNgramFSA.Parse(lex.Graph.ToCharArray(), currentNgramSize);
            }
        }

        private void ParseTextHMM(Text text)
        {
            if (localHMM == null)
                localHMM = new Dictionary<string, int>();
            else
                localHMM.Clear();

            localHMMFSA = new FiniteStateAutomoton<string>();

            foreach (var exp in text)
            {
                var morphArray = exp.Select(m => m.Graph).ToArray();

                localHMMFSA.Parse(morphArray, currentMarkovChainSize);
            }            
        }

        #endregion

        #region Generate

        private void GenerateWords(int number)
        {
            if (CreatedLexicon == null)
                CreatedLexicon = new Lexicon();

            // TODO: decide whether to generate Ngram FSA if it's null...

            if (localNgramFSA == null)
            {
                ParseLexiconNgramFSA(LocalLexicon);
            }

            var random = new Random();

            for (int w = 0; w < number; w++)
            {
                var word = localNgramFSA.GenerateRandomChain(random);

                CreatedLexicon.Add(new string(word.ToArray()));
            }
        }

        private void GenerateWords_Click(object sender, RoutedEventArgs e)
        {
            GenerateWords(currentGenerateWordsSize);
            UpdateGeneratedWordGrid();
        }

        private void GenerateLines_Click(object sender, RoutedEventArgs e)
        {
            var builder = new StringBuilder();
            var random = new Random();

            for (int l = 0; l < currentGenerateLinesSize; l++)
            {
                var words = localHMMFSA.GenerateRandomChain(random);

                string line = string.Join(" ", words);

                builder.Append(line);
                builder.AppendLine();
            }

            UpdateTextView(builder.ToString(), ViewMode.Generated);
        }

        private void ClearGeneratedWords_Click(object sender, RoutedEventArgs e)
        {
            if (CreatedLexicon == null)
                CreatedLexicon = new Lexicon();
            else
                CreatedLexicon.Clear();

            UpdateGeneratedWordGrid();
        }


        #endregion
    }
}
