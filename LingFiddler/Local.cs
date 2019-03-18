using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace LingFiddler
{
    internal class LingMachine
    {
        protected Lx.Text text;
        public Lx.Text Text
        {
            get
            {
                if (text == null)
                    text = new Lx.Text();

                return text;
            }

            set
            {
                text = value;
            }
        }

        protected Lx.Lexicon lexicon;
        public Lx.Lexicon Lexicon
        {
            get
            {
                if (lexicon == null)
                    lexicon = new Lx.Lexicon();

                return lexicon;
            }

            set
            {
                lexicon = value;
            }
        }

        public Regex LinePattern { get; set; }
        public Regex WordPattern { get; set; }
        public Regex PunctuationPattern { get; set; }

        public Lx.FiniteStateAutomoton<char> WordModel;
        public Lx.FiniteStateAutomoton<string> TextModel;

        public Lx.Text GeneratedText { get; set; }
        public Lx.Lexicon GeneratedLexicon { get; set; }

        public LingMachine()
        {

        }

        public void ParseText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return;

            MainWindow.Instance.ParseText.IsEnabled = false;
            MainWindow.Instance.BackgroundProgress.Maximum = MainWindow.Instance.countLine;

            var worker = new BackgroundWorker();
            worker.DoWork += ParseText_DoWork;
            worker.ProgressChanged += MainWindow.Instance.UpdateProgressBar;
            worker.RunWorkerCompleted += ParseText_WorkCompleted;
            worker.WorkerReportsProgress = true;

            worker.RunWorkerAsync(text);
        }

        private void ParseText_DoWork(object sender, DoWorkEventArgs e)
        {
            Lexicon.Clear();
            var expElementPattern = new Regex($"({PunctuationPattern.ToString()})|({WordPattern.ToString()})");
            var whiteSpacePattern = new Regex(@"[\s\n\r]+", RegexOptions.Singleline | RegexOptions.Multiline);

            var worker = sender as BackgroundWorker;
            var text = e.Argument as string;
            int progress = 0;
            string state = string.Empty;

            // TODO: handling of paragraph breaks and section headers, etc

            foreach (Match l in LinePattern.Matches(text))
            {
                //store line, section up into words and punctuation
                string cleanedLine = l.Value.Trim();
                cleanedLine = whiteSpacePattern.Replace(cleanedLine, " ");
                state = cleanedLine;

                var thisExpression = new Lx.Expression(cleanedLine);

                foreach (Match m in expElementPattern.Matches(thisExpression.Graph))
                {
                    if (m.Groups.Count > 0)
                    {
                        if (string.IsNullOrEmpty(m.Groups[1].Value))
                        {
                            thisExpression.Add(Text.Lexicon.Add(m.Groups[2].Value));                            
                        }
                        else
                        {
                            thisExpression.Add(Text.Morphosyntax.Add(m.Groups[1].Value));
                        }
                    }                    
                }

                Text.Add(thisExpression);
                worker.ReportProgress(++progress, state);
            }

            UpdateLocalLexicon();
        }

        private void ParseText_WorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled && e.Error == null)
            {
                MainWindow.Instance.BackgroundProgress.Value = 0;
                MainWindow.Instance.BackgroundStatus.Text = string.Empty;
                MainWindow.Instance.UpdateWordGrid();

                MainWindow.Instance.ParseNgrams.IsEnabled = true;
                MainWindow.Instance.ParseTextModel.IsEnabled = true;
            }
            else if (e.Cancelled)
            {
                // action cancelled
            }
            else
            {
                // an error occurred
            }

            MainWindow.Instance.ParseText.IsEnabled = true;
        }

        public void ConstructWordModel(int ngramLength)
        {
            ConstructWordModel(Lexicon, ngramLength);
        }

        public void ConstructWordModel(Lx.Lexicon lexicon, int ngramLength)
        {
            if (lexicon == null || ngramLength < 1)
                return;

            MainWindow.Instance.ParseNgrams.IsEnabled = false;
            MainWindow.Instance.GenerateWords.IsEnabled = false;
            MainWindow.Instance.BackgroundProgress.Maximum = lexicon.Count;

            WordModel = new Lx.FiniteStateAutomoton<char>();

            var arguments = new WordModelArguments()
            {
                Model = WordModel,
                Lexicon = lexicon,
                NgramLength = ngramLength
            };

            var worker = new BackgroundWorker();
            worker.DoWork += WordModel_DoWork;
            worker.ProgressChanged += MainWindow.Instance.UpdateProgressBar;
            worker.RunWorkerCompleted += WordModel_WorkCompleted;
            worker.WorkerReportsProgress = true;

            worker.RunWorkerAsync(arguments);
        }

        private struct WordModelArguments
        {
            public Lx.FiniteStateAutomoton<char> Model;
            public Lx.Lexicon Lexicon;
            public int NgramLength;
        }

        private void WordModel_DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;

            var arguments = (WordModelArguments)e.Argument;
            int progress = 0;

            // TODO: give some sort of admonishment if lexicon is empty

            //if (localNgrams == null)
            //    localNgrams = new Dictionary<string, int>();
            //else
            //    localNgrams.Clear();

            foreach (var lex in arguments.Lexicon.Keys)
            {
                //foreach (var ngram in Lx.Ngram.Parse(lex.Graph, currentNgramSize))
                //{
                //    if (localNgrams.ContainsKey(ngram))
                //    {
                //        localNgrams[ngram] += 1;
                //    }
                //    else
                //    {
                //        localNgrams.Add(ngram, 1);
                //    }
                //}

                arguments.Model.Parse(lex.Graph.ToCharArray(), arguments.NgramLength);

                string state = lex.Graph;
                worker.ReportProgress(++progress, state);
            }

            //e.Result = arguments;
        }

        private void WordModel_WorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled && e.Error == null)
            {
                MainWindow.Instance.BackgroundProgress.Value = 0;
                MainWindow.Instance.BackgroundStatus.Text = string.Empty;

                MainWindow.Instance.GenerateWords.IsEnabled = true;
            }
            else if (e.Cancelled)
            {
                // action cancelled
            }
            else
            {
                // an error occurred
            }

            MainWindow.Instance.ParseNgrams.IsEnabled = true;
        }

        internal void ConstructTextModel(int chainLength)
        {
            ConstructTextModel(Text, chainLength);
        }


        internal void ConstructTextModel(Lx.Text text, int chainLength)
        {
            if (text == null || chainLength < 1)
                return;

            MainWindow.Instance.ParseTextModel.IsEnabled = false;
            MainWindow.Instance.GenerateLines.IsEnabled = false;
            MainWindow.Instance.BackgroundProgress.Maximum = text.Count;

            TextModel = new Lx.FiniteStateAutomoton<string>();

            var arguments = new TextModelArguments()
            {
                Model = TextModel,
                Text = text,
                ChainLength = chainLength
            };

            var worker = new BackgroundWorker();
            worker.DoWork += TextModel_DoWork;
            worker.ProgressChanged += MainWindow.Instance.UpdateProgressBar;
            worker.RunWorkerCompleted += TextModel_WorkCompleted;
            worker.WorkerReportsProgress = true;

            worker.RunWorkerAsync(arguments);
        }

        private struct TextModelArguments
        {
            public Lx.FiniteStateAutomoton<string> Model;
            public Lx.Text Text;
            public int ChainLength;
        }

        private void TextModel_DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;
            var arguments = (TextModelArguments)e.Argument;
            int progress = 0;

            foreach(var exp in arguments.Text)
            {
                var morphArray = exp.Select(m => m.Graph).ToArray();
                arguments.Model.Parse(morphArray, arguments.ChainLength);
            
                string state = exp.Graph;
                worker.ReportProgress(++progress, state);
            }
        }

        private void TextModel_WorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled && e.Error == null)
            {
                MainWindow.Instance.BackgroundProgress.Value = 0;
                MainWindow.Instance.BackgroundStatus.Text = string.Empty;

                MainWindow.Instance.GenerateLines.IsEnabled = true;
            }
            else if (e.Cancelled)
            {
                // action cancelled
            }
            else
            {
                // an error occurred
            }

            MainWindow.Instance.ParseTextModel.IsEnabled = true;
        }



        private void UpdateLocalLexicon()
        {
            foreach (var m in Text.Lexicon.Keys)
            {
                // this is problematic as it creates new instances
                Lexicon.Add(m.Graph.ToLower(), Text.Lexicon[m]);
            }
        }

        #region Generate

        public void GenerateWords(int number)
        {
            if (GeneratedLexicon == null)
                GeneratedLexicon = new Lx.Lexicon();

            if (WordModel == null)
            {
                // Word model hasn't been built yet
                return;
            }

            if (WordModel.Start.Transitions.Count == 0)
            {
                // Word Model has no transitions from Start state.
                return;
            }

            var worker = new BackgroundWorker();
            worker.DoWork += GenerateWord_DoWork;
            //worker.ProgressChanged += MainWindow.Instance.UpdateProgressBar;
            worker.RunWorkerCompleted += GenerateWord_WorkCompleted;
            //worker.WorkerReportsProgress = true;

            worker.RunWorkerAsync(number);
        }

        private void GenerateWord_DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;
            int number = (int)e.Argument;
            //int progress = 0;

            var random = new Random();

            for (int w = 0; w < number; w++)
            {
                var word = WordModel.GenerateRandomChain(random);

                GeneratedLexicon.Add(new string(word.ToArray()));
            }
        }

        private void GenerateWord_WorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled && e.Error == null)
            {
            }
            else if (e.Cancelled)
            {
                // action cancelled
            }
            else
            {
                // an error occurred
            }
        }

        public void GenerateLines(int number)
        {
            if (GeneratedText is null)
                GeneratedText = new Lx.Text();

            //var builder = new StringBuilder();
            var random = new Random();

            for (int l = 0; l < number; l++)
            {
                var words = TextModel.GenerateRandomChain(random);
                var line = new Lx.Expression(string.Join(" ", words));
                GeneratedText.Add(line);
            }
        }

        public void ClearGeneratedText()
        {
            GeneratedText.Clear();
        }

        public void ClearGeneratedLexicon()
        {
            GeneratedLexicon.Clear();
        }

        #endregion
    }
}
