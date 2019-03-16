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

        //public int sizeNgram = 2;
        //public int sizeMarkovChain = 2;
        //public int numberGenerateWords = 10;
        //public int numberGenerateLines = 5;

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

            Lexicon.Clear();
            var expElementPattern = new Regex($"({PunctuationPattern.ToString()})|({WordPattern.ToString()})");
            var whiteSpacePattern = new Regex(@"[\s\n\r]+", RegexOptions.Singleline | RegexOptions.Multiline);

            // TODO: handling of paragraph breaks and section headers, etc

            foreach (Match l in LinePattern.Matches(text))
            {
                //store line, section up into words and punctuation
                string cleanedLine = l.Value.Trim();
                cleanedLine = whiteSpacePattern.Replace(cleanedLine, " ");

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
            }

            UpdateLocalLexicon();
        }

        public void ConstructWordModel(int ngramLength)
        {
            ConstructWordModel(Lexicon, ngramLength);
        }

        public void ConstructWordModel(Lx.Lexicon lexicon, int ngramLength)
        {
            // TODO: give some sort of admonishment if lexicon is empty

            //if (localNgrams == null)
            //    localNgrams = new Dictionary<string, int>();
            //else
            //    localNgrams.Clear();

            WordModel = new Lx.FiniteStateAutomoton<char>(ngramLength);

            foreach (var lex in lexicon.Keys)
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

                WordModel.Parse(lex.Graph.ToCharArray());
            }
        }

        internal void ConstructTextModel(int chainLength)
        {
            ConstructTextModel(Text, chainLength);
        }


        internal void ConstructTextModel(Lx.Text text, int chainLength)
        {
            MainWindow.Instance.ParseTextModel.IsEnabled = false;
            MainWindow.Instance.BackgroundProgress.Maximum = text.Count;

            TextModel = new Lx.FiniteStateAutomoton<string>(chainLength);

            object[] arguments = { TextModel, text };

            var worker = new BackgroundWorker();
            worker.DoWork += TextModel_DoWork;
            worker.ProgressChanged += MainWindow.Instance.UpdateProgressBar;
            worker.RunWorkerCompleted += TextModel_WorkCompleted;
            worker.WorkerReportsProgress = true;

            worker.RunWorkerAsync(arguments);
        }

        private void TextModel_DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;
            var arguments = e.Argument as object[];
            var model = arguments[0] as Lx.FiniteStateAutomoton<string>;
            var text = arguments[1] as Lx.Text;

            for (int i = 0; i < text.Count; i++)
            {
                var morphArray = text[i].Select(m => m.Graph).ToArray();
                TextModel.Parse(morphArray);
            
                int progress = i;
                string state = text[i].Graph;

                worker.ReportProgress(i, state);
            }
        }

        private void TextModel_WorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled && e.Error == null)
            {
                MainWindow.Instance.BackgroundProgress.Value = 0;
                MainWindow.Instance.BackgroundStatus.Text = string.Empty;
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

            var random = new Random();

            for (int w = 0; w < number; w++)
            {
                var word = WordModel.GenerateRandomChain(random);

                GeneratedLexicon.Add(new string(word.ToArray()));
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
