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
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace LingFiddler
{
    public partial class MainWindow : Window
    {
        internal static MainWindow Instance { get; private set; }
        internal LingMachine CurrentLanguage;

        #region local Variables
        internal string CurrentText { get; set; }
        internal HashSet<char> CurrentCharSet { get; set; }

        internal int sizeNgram;
        internal int numberGenerateWords;
        internal int sizeMarkovChain;
        internal int numberGenerateLines;

        internal string patternLine;
        internal string patternWord;
        internal string patternPunctuation;

        public string CountChar
        {
            get { return CurrentText.Length.ToString(); }
        }

        public string CountCharUnique
        {
            get { return CurrentCharSet.Count.ToString(); }
        }

        internal int countWord = 0;
        public string CountWord
        {
            get { return countWord.ToString(); }
        }

        public string CountWordUnique
        {
            get
            {
                return CurrentLanguage.Lexicon.UniqueWordCount.ToString();
            }
        }

        internal int countLine = 0;
        public string CountLine
        {
            get { return countLine.ToString(); }
        }

        internal int countParagraph = 0;
        public string CountParagraph
        {
            get { return countParagraph.ToString(); }
        }

        #endregion

        #region DataGrid Views
        internal enum ViewMode { Text, Lines, Generated }
        internal ViewMode currentViewMode = ViewMode.Text;

        internal enum GridMode { Words, Ngrams, Generated }
        internal GridMode currentGridMode = GridMode.Words;

        public class MorphemeView : ObservableCollection<MorphemeView.MorphemeViewItem>, IDisposable
        {
            public class MorphemeViewItem
            {
                public readonly Lx.Morpheme Morpheme;
                public readonly Lx.Lexicon Lexicon;
                public readonly MorphemeView ParentView;
                public string Graph
                {
                    get { return Morpheme.Graph; }
                    set
                    {
                        if (Lexicon != null)
                        {
                            var morph = Lexicon.Keys.FirstOrDefault(m => m.Graph == value);

                            if (morph != null)
                            {
                                Lexicon[morph] += Lexicon[Morpheme];
                                Lexicon.Remove(Morpheme);
                                ParentView.Remove(this);
                                CollectionViewSource.GetDefaultView(ParentView).Refresh();
                            }
                            else
                            {
                                int weight = Lexicon[Morpheme];
                                Lexicon.Remove(Morpheme);
                                Morpheme.Graph = value;
                                Lexicon.Add(Morpheme, weight);
                            }
                        }
                        else
                        {
                            Morpheme.Graph = value;
                        }
                    }
                }
                public int Length { get { return Graph.Length; } }
                public int Frequency
                {
                    get
                    {
                        return Lexicon != null && Lexicon.ContainsKey(Morpheme) ? Lexicon[Morpheme] : 0;
                    }
                    set
                    {
                        if (Lexicon != null && Lexicon.ContainsKey(Morpheme))
                        {
                            Lexicon[Morpheme] = value;
                        }                            
                    }
                }

                public MorphemeViewItem(Lx.Morpheme morpheme, Lx.Lexicon lexicon, MorphemeView view)
                {
                    Morpheme = morpheme;
                    Lexicon = lexicon;
                    ParentView = view;
                }
            }

            public MorphemeView()
            {
                this.CollectionChanged += ItemUpdated;
            }

            public bool AddNewItem(Lx.Morpheme morpheme, Lx.Lexicon lexicon)
            {
                if (this.Any(m => m.Morpheme == morpheme && m.Lexicon == lexicon))
                {
                    // this is already in the view collection
                    return false;
                }
                else
                {
                    Add(new MorphemeViewItem(morpheme, lexicon, this));
                    return true;
                }
            }

            public static MorphemeView GetMorphemeView(Lx.Lexicon lexicon)
            {
                var list = new MorphemeView();

                foreach (var m in lexicon.Keys.OrderBy(m => m.Graph))
                {
                    list.AddNewItem(m, lexicon);
                }

                return list;
            }

            private void ItemUpdated(object sender, NotifyCollectionChangedEventArgs e)
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        foreach (MorphemeViewItem item in e.NewItems)
                        {
                            //int index = e.NewStartingIndex;
                            //Lx.Lexicon lexicon = this[index].Lexicon;

                            //item.Lexicon
                        }
                        break;
                    case NotifyCollectionChangedAction.Move:
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        foreach (MorphemeViewItem item in e.OldItems)
                        {
                            item.Lexicon.Remove(item.Morpheme);
                        }
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        break;
                }
            }

            public void Dispose()
            {
                this.CollectionChanged -= ItemUpdated;
            }
        }


        public List<Lx.Morpheme> ViewListGeneratedWord
        {
            get
            {
                return CurrentLanguage.GeneratedLexicon != null ? CurrentLanguage.GeneratedLexicon.Keys.ToList() : null;
            }
        }

        public List<Lx.Morpheme> SelectedWords { get; set; }

        public class NgramView
        {
            public string Onset { get; set; }
            public string Coda { get; set; }
            public string Value { get; set; }
            public int Weight { get; set; }

            public static List<NgramView> GetViewList(Lx.FiniteStateAutomoton<char> ngrams)
            {
                if (ngrams == null)
                    return null;

                var outList = new List<NgramView>();

                //foreach (var ngram in ngrams.Keys)
                //{
                //    outList.Add(
                //        new NgramView()
                //        {
                //            Onset = ngram.Substring(0, 1),
                //            Coda = ngram.Substring(ngram.Length - 1),
                //            Value = ngram,
                //            Weight = ngrams[ngram]
                //        }
                //    );
                //}

                return outList.OrderBy(n => n.Value).ToList();
            }
        }
        public List<NgramView> ViewListNgram;

        #endregion

        public MainWindow()
        {
            Instance = this;
            InitializeComponent();
            InitializeApp();
        }

        private void InitializeApp()
        {
            CurrentLanguage = new LingMachine();
            CurrentCharSet = new HashSet<char>();

            //--

            sizeNgram = 2;
            numberGenerateWords = 10;
            sizeMarkovChain = 2;
            numberGenerateLines = 5;

            patternLine = @".+?[\.\?\!]+";
            patternWord = @"[^\W0-9_]+";
            patternPunctuation = @"[',.:;]+";

            //--

            SizeNgram.Text = sizeNgram.ToString();
            SizeGenerateWords.Text = numberGenerateWords.ToString();
            SizeMarkovChain.Text = sizeMarkovChain.ToString();
            SizeGenerateLines.Text = numberGenerateLines.ToString();

            PatternLine.Text = patternLine;
            PatternWord.Text = patternWord;
            PatternPunctuation.Text = patternPunctuation;

            ParseNgrams.IsEnabled = false;
            GenerateWords.IsEnabled = false;
            ParseTextModel.IsEnabled = false;
            GenerateLines.IsEnabled = false;
        }

        private void LoadText_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new Microsoft.Win32.OpenFileDialog();

            if (openDialog.ShowDialog() == true)
            {
                string loadedText = File.ReadAllText(openDialog.FileName, Encoding.UTF8);
                UpdateTextView(loadedText, ViewMode.Text);
            }            
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

                var textChars = CurrentText.ToCharArray();
                CurrentCharSet = new HashSet<char>(textChars.Distinct());

                // TODO: bind these values directly in XAML
                CountChars.Text = CountChar;
                CountUniqueChars.Text = CountCharUnique;
                //

                UpdateWordCount();
            }
        }

        private void CurrentWordPattern_Updated(object sender, TextChangedEventArgs e)
        {
            try
            {
                CurrentLanguage.WordPattern = new Regex(PatternWord.Text);
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
                CurrentLanguage.LinePattern = new Regex(PatternLine.Text, RegexOptions.Singleline);
                UpdateWordCount();
            }
            catch (Exception ex)
            {
            }
        }

        private void CurrentPuncuationPattern_Updated(object sender, TextChangedEventArgs e)
        {
            try
            {
                CurrentLanguage.PunctuationPattern = new Regex(PatternPunctuation.Text, RegexOptions.Singleline);
            }
            catch (Exception ex)
            {
            }
        }

        private void SizeNgram_Updated(object sender, TextChangedEventArgs e)
        {
            int n = sizeNgram;

            if (!int.TryParse(SizeNgram.Text, out n))
            {
                // Return an error message about validation
                SizeNgram.Text = sizeNgram.ToString();
            }
            else
            {
                sizeNgram = n;
            }

            //CurrentLanguage.sizeNgram = sizeNgram;
        }

        private void SizeMarkovChain_Updated(object sender, TextChangedEventArgs e)
        {
            int n = sizeMarkovChain;

            if (!int.TryParse(SizeMarkovChain.Text, out n))
            {
                // Return an error message about validation
                SizeMarkovChain.Text = sizeMarkovChain.ToString();
            }
            else
            {
                sizeMarkovChain = n;
            }

            //CurrentLanguage.sizeMarkovChain = sizeMarkovChain;
        }

        private void NumberGenerateWords_Updated(object sender, TextChangedEventArgs e)
        {
            int n = numberGenerateWords;

            if (!int.TryParse(SizeGenerateWords.Text, out n))
            {
                // Return an error message about validation
                SizeGenerateWords.Text = numberGenerateWords.ToString();
            }
            else
            {
                numberGenerateWords = n;
            }

            //CurrentLanguage.numberGenerateWords = numberGenerateWords;
        }

        private void NumberGenerateLines_Updated(object sender, TextChangedEventArgs e)
        {
            int n = numberGenerateLines;

            if (!int.TryParse(SizeGenerateLines.Text, out n))
            {
                // Return an error message about validation
                SizeGenerateLines.Text = numberGenerateLines.ToString();
            }
            else
            {
                numberGenerateLines = n;               
            }

            //CurrentLanguage.numberGenerateLines = numberGenerateLines;
        }

        #endregion

        #region Update ProgressBar
        public void UpdateProgressBar(object sender, ProgressChangedEventArgs e)
        {
            BackgroundProgress.Value = e.ProgressPercentage;
            BackgroundStatus.Text = e.UserState.ToString();
        }
        #endregion

        #region Update Text Box

        private void UpdateTextView(string text, ViewMode mode)
        {
            currentViewMode = mode;
            TextBlock.Text = text;
        }

        private void UpdateWordCount()
        {
            countWord = CurrentLanguage.WordPattern.Matches(CurrentText).Count;
            countLine = CurrentLanguage.LinePattern.Matches(CurrentText).Count;

            // TODO: bind these controls to these values
            CountWords.Text = CountWord;
            CountUniqueWords.Text = CountWordUnique;
            CountLines.Text = CountLine;
        }

        #endregion

        #region Update Word Grid

        internal void UpdateWordGrid()
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

            var morphemeView = MorphemeView.GetMorphemeView(CurrentLanguage.Lexicon);            
            WordGrid.ItemsSource = morphemeView;
        }

        internal void UpdateNgramGrid(Lx.FiniteStateAutomoton<char> source, List<NgramView> target)
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

        internal void UpdateGeneratedWordGrid()
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
            WordGrid.ItemsSource = ViewListGeneratedWord;
        }

        private void WordGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedWords == null)
                SelectedWords = new List<Lx.Morpheme>();

            foreach (MorphemeView.MorphemeViewItem m in e.RemovedItems)
            {
                SelectedWords.Remove(m.Morpheme);
            }

            foreach (MorphemeView.MorphemeViewItem m in e.AddedItems)
            {
                SelectedWords.Add(m.Morpheme);
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedWords == null)
                SelectedWords = new List<Lx.Morpheme>();

            SelectedWords.ForEach(w => CurrentLanguage.Lexicon.Remove(w));
            SelectedWords.Clear();

            UpdateWordGrid();
        }

        #endregion

        private void MyTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // When the RichTextBox loses focus the user can no longer see the selection.
            // This is a hack to make the RichTextBox think it did not lose focus.
            e.Handled = true;
        }

        #region Show

        private void ShowLines_Click(object sender, RoutedEventArgs e)
        {
            UpdateTextView(CurrentLanguage.Text.ToString(), ViewMode.Lines);
        }

        private void ShowText_Click(object sender, RoutedEventArgs e)
        {
            UpdateTextView(CurrentText, ViewMode.Text);
        }

        private void ShowGeneratedLines_Click(object sender, RoutedEventArgs e)
        {
            UpdateTextView(CurrentLanguage.GeneratedText.ToString(), ViewMode.Generated);
        }

        private void ShowWords_Click(object sender, RoutedEventArgs e)
        {
            UpdateWordGrid();
        }

        private void ShowNgrams_Click(object sender, RoutedEventArgs e)
        {
            UpdateNgramGrid(CurrentLanguage.WordModel, ViewListNgram);
        }

        private void ShowCreatedWords_Click(object sender, RoutedEventArgs e)
        {
            UpdateGeneratedWordGrid();
        }

        #endregion

        #region Parse

        private void ParseText_Click(object sender, RoutedEventArgs e)
        {
            CurrentLanguage.ParseText(CurrentText);            
        }

        private void ParseNgrams_Click(object sender, RoutedEventArgs e)
        {
            CurrentLanguage.ConstructWordModel(sizeNgram);
            //UpdateNgramGrid(CurrentLanguage.WordModel, ViewListNgram);
        }

        private void ParseLines_Click(object sender, RoutedEventArgs e)
        {
            CurrentLanguage.ConstructTextModel(sizeMarkovChain);
        }

        #endregion

        #region Generate

        private void GenerateLines_Click(object sender, RoutedEventArgs e)
        {
            CurrentLanguage.GenerateLines(numberGenerateLines);
            UpdateTextView(CurrentLanguage.GeneratedText.ToString(), ViewMode.Generated);
        }

        private void GenerateWords_Click(object sender, RoutedEventArgs e)
        {
            CurrentLanguage.GenerateWords(numberGenerateWords);
            UpdateGeneratedWordGrid();
        }

        private void ClearGeneratedWords_Click(object sender, RoutedEventArgs e)
        {
            CurrentLanguage.ClearGeneratedLexicon();
            UpdateGeneratedWordGrid();
        }

        private void ClearGeneratedText_Click(object sender, RoutedEventArgs e)
        {
            CurrentLanguage.ClearGeneratedText();
            UpdateTextView(CurrentLanguage.GeneratedText.ToString(), ViewMode.Generated);
        }

        #endregion
    }
}
