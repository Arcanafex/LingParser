using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data.SQLite;
using System.Linq;
using System.Text;
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

namespace LingFiddler
{
    /// <summary>
    /// Interaction logic for TextEntry.xaml
    /// </summary>
    public partial class TextEntry : UserControl
    {
        #region local Variables
        internal string CurrentText { get; set; }
        internal HashSet<char> CurrentCharSet { get; set; }
        internal string ConsoleText { get; set; }

        internal int sizeNgram;
        internal int numberGenerateWords;
        internal int sizeMarkovChain;
        internal int numberGenerateLines;

        internal string patternParagraph;
        internal string patternLine;
        internal string patternWord;
        internal string patternPunctuation;

        //public int CountChar
        //{
        //    get { return CurrentText.Length; }
        //}

        //public int CountCharUnique
        //{
        //    get { return CurrentCharSet.Count; }
        //}
        //public int CountWord { get; private set; }
        //public int CountWordUnique
        //{
        //    get
        //    {
        //        return CurrentLanguage.Lexicon.UniqueWordCount;
        //    }
        //}
        //public int CountLine { get; private set; }

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

        internal enum GridMode { Words, Ngrams, Generated, Script, Orthography }
        internal GridMode currentGridMode = GridMode.Words;

        internal void ChangeMode(ViewMode mode)
        {
            ViewMode lastMode = currentViewMode;
            currentViewMode = mode;
        }

        internal void ChangeMode(GridMode mode)
        {
            GridMode lastMode = currentGridMode;
            currentGridMode = mode;

            switch (mode)
            {
                case GridMode.Ngrams:
                    NodeList.Visibility = Visibility.Visible;
                    GlyphList.Visibility = Visibility.Collapsed;
                    TransitionGrid.Visibility = Visibility.Visible;
                    WordGrid.Visibility = Visibility.Collapsed;
                    break;
                case GridMode.Script:
                    NodeList.Visibility = Visibility.Collapsed;
                    GlyphList.Visibility = Visibility.Visible;
                    TransitionGrid.Visibility = Visibility.Collapsed;
                    WordGrid.Visibility = Visibility.Collapsed;
                    break;
                case GridMode.Orthography:
                    NodeList.Visibility = Visibility.Collapsed;
                    GlyphList.Visibility = Visibility.Visible;
                    TransitionGrid.Visibility = Visibility.Collapsed;
                    WordGrid.Visibility = Visibility.Collapsed;
                    break;
                default:
                    NodeList.Visibility = Visibility.Collapsed;
                    GlyphList.Visibility = Visibility.Collapsed;
                    TransitionGrid.Visibility = Visibility.Collapsed;
                    WordGrid.Visibility = Visibility.Visible;
                    break;
            }

        }

        public class TextViewModel
        {
            public List<DiscourseViewItem> Paragraphs;
        }

        public class DiscourseViewItem
        {
            public List<ExpressionViewItem> Expressions { get; set; }
            //public string Graph { get; set; }
        }

        public class ExpressionViewItem
        {
            public List<MorphViewItem> Morphemes { get; set; }
            //public List<MorphemeView.MorphemeViewItem> Morphemes { get; set; }
            //public string Graph { get; set; }
        }

        public class MorphViewItem
        {
            public string Graph { get; set; }
        }

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

        internal TextViewModel TextModel;

        internal void DoTheThing()
        {
            TextModel = new TextViewModel
            {
                Paragraphs = new List<DiscourseViewItem>()
            };

            foreach (var p in CurrentLanguage.Text.Discourse)
            {
                var paragraph = new DiscourseViewItem
                {
                    //Graph = p.ToString()
                    Expressions = new List<ExpressionViewItem>()
                };
                TextModel.Paragraphs.Add(paragraph);

                foreach (var e in p.Expressions)
                {
                    var expression = new ExpressionViewItem()
                    {
                        //Graph = e.ToString()
                        //Morphemes = new List<MorphemeView.MorphemeViewItem>()
                        Morphemes = new List<MorphViewItem>()
                    };
                    paragraph.Expressions.Add(expression);

                    //var dummy = new MorphemeView();

                    foreach (var m in e.Sequence)
                    {
                        //var morpheme = new MorphemeView.MorphemeViewItem(m, CurrentLanguage.Text.Lexicon, dummy);
                        var morpheme = new MorphViewItem()
                        {
                            Graph = m.Graph
                        };
                        expression.Morphemes.Add(morpheme);
                    }
                }
            }
        }

        public class NgramView : ObservableCollection<NgramView.NgramViewItem>
        {
            public readonly Lx.FiniteStateAutomoton<Lx.Grapheme> StateMachine;

            internal NgramView(Lx.FiniteStateAutomoton<Lx.Grapheme> stateMachine)
            {
                if (stateMachine == null)
                    return;

                StateMachine = stateMachine;

                foreach (var item in StateMachine.Transitions.OrderBy(t => t.Key.ToString()))
                {
                    var viewItem = AddNgramViewItem(item.Key);

                    foreach (var transition in item.Value.OrderBy(t => t.Key.ToString()))
                    {
                        viewItem.AddTransitionViewItem(transition.Value);
                    }
                }
            }

            public static NgramView GetNgramView(Lx.FiniteStateAutomoton<Lx.Grapheme> stateMachine)
            {
                var view = new NgramView(stateMachine);
                return view;
            }

            public NgramViewItem AddNgramViewItem(Lx.NodeChain<Lx.Grapheme> state)
            {
                var item = new NgramViewItem(this, state);
                Add(item);
                return item;
            }

            public class NgramViewItem
            {
                public readonly Lx.NodeChain<Lx.Grapheme> StateChain;
                public readonly NgramView ParentView;

                public ObservableCollection<TransitionViewItem> TransitionView;

                public string Value
                {
                    get
                    {
                        return StateChain.ToString();
                    }
                }

                internal NgramViewItem(NgramView parentView, Lx.NodeChain<Lx.Grapheme> stateChain)
                {
                    ParentView = parentView;
                    StateChain = stateChain;

                    TransitionView = new ObservableCollection<TransitionViewItem>();
                }

                internal void AddTransitionViewItem(Lx.Transition<Lx.Grapheme> transition)
                {
                    TransitionView.Add(new TransitionViewItem(this, transition));
                }
            }

            public class TransitionViewItem
            {
                public readonly NgramViewItem OriginState;
                public readonly Lx.Transition<Lx.Grapheme> Transition;

                private string coda;
                public string Coda
                {
                    get
                    {
                        return coda;
                    }
                    set
                    {
                        var updatedValue = value;

                        //if (updatedValue.Length > 0)
                        //{
                        //    // TODO: 
                        //    // 1. find glyphs in current script
                        //    // 2. select matching maximal graphemes

                        //    var glyph = Instance.CurrentLanguage.Script.AddGlyph(updatedValue.ToCharArray().First());
                        //    var segment = new Lx.Grapheme(glyph);

                        //    if (Transition.EndState.Value.Graph != segment.Graph)
                        //    {
                        //        var model = Transition.ParentModel;
                        //        var node = model.AddNode(segment);

                        //        if (model.Transitions[Transition.Chain].ContainsKey(node))
                        //        {
                        //            model.Transitions[Transition.Chain][node].MergeTransition(Transition);
                        //            OriginState.TransitionView.Remove(this);
                        //            CollectionViewSource.GetDefaultView(MainWindow.Instance.WordGrid.ItemsSource).Refresh();
                        //        }
                        //        else
                        //        {
                        //            model.Transitions[Transition.Chain].Remove(Transition.EndState);
                        //            Transition.EndState = node;
                        //            model.Transitions[Transition.Chain].Add(node, Transition);
                        //            coda = Transition.EndState.ToString();
                        //            MainWindow.Instance.UpdateNgramGrid(Transition.ParentModel);
                        //        }                                
                        //    }
                        //}
                    }
                }

                public int Weight
                {
                    get
                    {
                        return Transition.Weight;
                    }
                    set
                    {
                        Transition.Weight = value;
                    }
                }

                internal TransitionViewItem(NgramViewItem originState, Lx.Transition<Lx.Grapheme> transition)
                {
                    OriginState = originState;
                    Transition = transition;

                    switch (transition.EndState.Type)
                    {
                        case Lx.NodeType.Start:
                            coda = "[START]";
                            break;

                        case Lx.NodeType.Value:
                            coda = transition.EndState.Value.ToString();
                            break;

                        case Lx.NodeType.End:
                            coda = "[END]";
                            break;
                    }
                }
            }
        }

        public class ScriptView : ObservableCollection<ScriptView.ScriptViewItem>
        {
            public static ScriptView GetScriptView(Lx.Script script)
            {
                var view = new ScriptView(script);
                return view;
            }

            internal ScriptView(Lx.Script script)
            {

            }

            public class ScriptViewItem
            {
                public ScriptView ParentView { get; set; }
                public Lx.Glyph Glyph { get; set; }
                public string Value
                {
                    get;
                }
            }
        }

        public class OrthographyView
        {

        }

        #endregion

        private TextEntryViewModel m_view { get; set; }

        public TextEntry()
        {
            InitializeComponent();

            m_view = new TextEntryViewModel();
            DataContext = m_view;
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

            patternParagraph = @".+?\r?\n\r?\n";
            patternLine = @".+?([\.\?\!]+|$)";
            patternWord = @"[^\W0-9_]+";
            patternPunctuation = @"\s?[',.:;]+\s?";

            //--

            SizeNgram.Text = sizeNgram.ToString();
            SizeGenerateWords.Text = numberGenerateWords.ToString();
            SizeMarkovChain.Text = sizeMarkovChain.ToString();
            SizeGenerateLines.Text = numberGenerateLines.ToString();

            PatternParagraph.Text = patternParagraph;
            PatternLine.Text = patternLine;
            PatternWord.Text = patternWord;
            PatternPunctuation.Text = patternPunctuation;

            ParseNgrams.IsEnabled = false;
            GenerateWords.IsEnabled = false;
            ParseTextModel.IsEnabled = false;
            GenerateLines.IsEnabled = false;

            ShowText.IsSelected = true;
        }

        private void LoadText_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new Microsoft.Win32.OpenFileDialog();

            if (openDialog.ShowDialog() == true)
            {
                CurrentLanguage.LoadText(openDialog.FileName);
                //string loadedText = File.ReadAllText(openDialog.FileName, Encoding.UTF8);
                //UpdateTextView(loadedText, ViewMode.Text);
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

        private void CurrentParagraphPattern_Updated(object sender, TextChangedEventArgs e)
        {
            try
            {
                CurrentLanguage.ParagraphPattern = new Regex(PatternParagraph.Text, RegexOptions.Singleline);
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
        }

        #endregion

        #region Update ProgressBar
        public void UpdateProgressBar(object sender, ProgressChangedEventArgs e)
        {
            BackgroundProgress.Value = e.ProgressPercentage;
            BackgroundStatus.Text = e.UserState.ToString();
        }

        internal bool console = false;
        internal void UpdateConsole()
        {
            var builder = new StringBuilder();

            foreach (var item in CurrentLanguage.WordModel.Transitions.OrderBy(t => t.Key.ToString()))
            {
                float total = item.Value.Values.Sum(t => t.Weight);
                builder.AppendLine($"{item.Key.ToString("-"),-20} : {100,-10}");

                foreach (var transition in item.Value.OrderBy(t => t.Key.ToString()))
                {
                    builder.AppendLine($"{"",-10} -{transition.Key.ToString(),-10} {(transition.Value.Weight / total) * 100,10:F2}%");
                }
            }

            ConsoleText = builder.ToString();
            UpdateTextView(ConsoleText, ViewMode.Generated);
        }
        #endregion

        #region Update Text Box

        internal void UpdateTextView(string text, ViewMode mode)
        {
            currentViewMode = mode;
            TextBlock.Text = text;
        }

        internal void UpdateWordCount()
        {
            countWord = CurrentLanguage != null && CurrentLanguage.WordPattern != null ? CurrentLanguage.WordPattern.Matches(CurrentText).Count : 0;
            countLine = CurrentLanguage != null && CurrentLanguage.LinePattern != null ? CurrentLanguage.LinePattern.Matches(CurrentText).Count : 0;

            // TODO: bind these controls to these values
            CountWords.Text = CountWord;
            CountUniqueWords.Text = CountWordUnique;
            CountLines.Text = CountLine;
        }

        #endregion

        #region Update Word Grid

        internal void UpdateWordGrid()
        {
            ChangeMode(GridMode.Words);

            var morphemeView = MorphemeView.GetMorphemeView(CurrentLanguage.Lexicon);
            WordGrid.ItemsSource = morphemeView;
        }

        internal void UpdateNgramGrid(Lx.FiniteStateAutomoton<Lx.Grapheme> source)
        {
            ChangeMode(GridMode.Ngrams);

            var view = NgramView.GetNgramView(source);
            NodeList.ItemsSource = view;
        }

        internal void UpdateGeneratedWordGrid()
        {
            ChangeMode(GridMode.Generated);

            var ViewListGeneratedWord = MorphemeView.GetMorphemeView(CurrentLanguage.GeneratedLexicon);
            WordGrid.ItemsSource = ViewListGeneratedWord;
        }

        #endregion

        private void MyTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // When the RichTextBox loses focus the user can no longer see the selection.
            // This is a hack to make the RichTextBox think it did not lose focus.
            e.Handled = true;
        }

        #region Show

        //private void ShowLines_Click(object sender, RoutedEventArgs e)
        //{
        //    UpdateTextView(CurrentLanguage.Text.ToString(), ViewMode.Lines);
        //}

        //private void ShowText_Click(object sender, RoutedEventArgs e)
        //{
        //    UpdateTextView(CurrentText, ViewMode.Text);
        //}

        //private void ShowGeneratedLines_Click(object sender, RoutedEventArgs e)
        //{
        //    if (CurrentLanguage.GeneratedText == null)
        //        CurrentLanguage.GeneratedText = new Lx.Text();

        //    UpdateTextView(CurrentLanguage.GeneratedText.ToString(), ViewMode.Generated);
        //}

        //private void ShowWords_Click(object sender, RoutedEventArgs e)
        //{
        //    UpdateWordGrid();
        //}

        //private void ShowNgrams_Click(object sender, RoutedEventArgs e)
        //{
        //    UpdateNgramGrid(CurrentLanguage.WordModel);
        //}

        //private void ShowCreatedWords_Click(object sender, RoutedEventArgs e)
        //{
        //    UpdateGeneratedWordGrid();
        //}


        private void SelectedNgram_Changed(object sender, SelectionChangedEventArgs e)
        {
            foreach (NgramView.NgramViewItem selectedItem in e.AddedItems)
            {
                TransitionGrid.ItemsSource = selectedItem.TransitionView;
            }
        }

        private void SelectedMode_Changed(object sender, SelectionChangedEventArgs e)
        {
            foreach (ComboBoxItem item in e.AddedItems)
            {
                switch (item.Name)
                {
                    case "ShowWords":
                        UpdateWordGrid();
                        break;
                    case "ShowNgrams":
                        UpdateNgramGrid(CurrentLanguage.WordModel);
                        break;
                    case "ShowCreatedWords":
                        UpdateGeneratedWordGrid();
                        break;
                    case "ShowGlyphs":
                        ChangeMode(GridMode.Script);
                        break;
                    case "ShowGraphemes":
                        ChangeMode(GridMode.Orthography);
                        break;
                    case "ShowText":
                        UpdateTextView(CurrentText, ViewMode.Text);
                        break;
                    case "ShowLines":
                        //UpdateTextView(CurrentLanguage.Text.ToString(), ViewMode.Lines);
                        TextBlock.Visibility = Visibility.Collapsed;
                        TextView.Visibility = Visibility.Visible;
                        TextView.ItemsSource = TextModel.Paragraphs;
                        break;
                    case "ShowGeneratedText":
                        if (CurrentLanguage.GeneratedText == null)
                            CurrentLanguage.GeneratedText = new Lx.Text();

                        UpdateTextView(CurrentLanguage.GeneratedText.ToString(), ViewMode.Generated);
                        break;
                    default:
                        break;
                }
            }
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
        }

        private void ClearGeneratedWords_Click(object sender, RoutedEventArgs e)
        {
            UpdateGeneratedWordGrid();
            CurrentLanguage.ClearGeneratedLexicon();
        }

        private void ClearGeneratedText_Click(object sender, RoutedEventArgs e)
        {
            CurrentLanguage.ClearGeneratedText();
            UpdateTextView(CurrentLanguage.GeneratedText.ToString(), ViewMode.Generated);
        }

        #endregion

        private void SelectedGlyph_Changed(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
