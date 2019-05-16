using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LingFiddler
{
    internal class LingMachine
    {
        public Lx.Text Text { get; set; }
        public Lx.Lexicon Lexicon { get; set; }
        public Lx.Script Script { get; set; }
        public Lx.Orthography Orthography { get; set; }

        public Regex ParagraphPattern { get; set; }
        public Regex LinePattern { get; set; }
        public Regex WordPattern { get; set; }
        public Regex PunctuationPattern { get; set; }

        public Lx.FiniteStateAutomoton<Lx.Grapheme> WordModel;
        public Lx.FiniteStateAutomoton<string> TextModel;

        public Lx.Text GeneratedText { get; set; }
        public Lx.Lexicon GeneratedLexicon { get; set; }

        public LingMachine()
        {
            Text = new Lx.Text();
            Lexicon = new Lx.Lexicon();
            GeneratedText = new Lx.Text();
            GeneratedLexicon = new Lx.Lexicon();
            Script = new Lx.Script();
            Orthography = new Lx.Orthography();
        }

        public void LoadText(string filePath)
        {
            //var worker = new BackgroundWorker();
            //worker.DoWork += LoadText_DoWork;
            //worker.ProgressChanged += LoadText_ProgressChanged;
            //worker.RunWorkerCompleted += LoadText_WorkCompleted;
            //worker.WorkerReportsProgress = true;

            var lines = File.ReadAllLines(filePath, Encoding.UTF8);

            //worker.RunWorkerAsync(lines);

            MainWindow.Instance.UpdateTextView(string.Join("\n", lines), MainWindow.ViewMode.Text);
        }

        private void LoadText_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            var text = (StringBuilder)e.UserState;
            MainWindow.Instance.UpdateTextView(text.ToString(), MainWindow.ViewMode.Text);
        }

        private void LoadText_DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;
            var text = (string[])e.Argument;
            var builder = new StringBuilder();
            int lineCounter = 0;

            foreach (var line in text)
            {
                builder.AppendLine(line);
                worker.ReportProgress(lineCounter++, builder);
                System.Threading.Thread.Sleep(10);
            }
        }

        private void LoadText_WorkCompleted(object sender, RunWorkerCompletedEventArgs e)
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
        }

        public void ParseText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return;

            MainWindow.Instance.ParseText.IsEnabled = false;
            MainWindow.Instance.BackgroundProgress.Maximum = MainWindow.Instance.countLine;
            //MainWindow.Instance.BackgroundProgress.Maximum = MainWindow.Instance.CountLine;

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
            foreach (Match p in ParagraphPattern.Matches(text))
            {
                string paragraphText = p.Value.Trim();
                paragraphText = whiteSpacePattern.Replace(paragraphText, " ");
                var paragraph = new Lx.Discourse();
                Text.AddLast(paragraph);

                foreach (Match l in LinePattern.Matches(paragraphText))
                {
                    //store line, section up into words and punctuation
                    string cleanedLine = l.Value.Trim();
                    //cleanedLine = whiteSpacePattern.Replace(cleanedLine, " ");
                    state = cleanedLine;

                    var expression = new Lx.Expression(cleanedLine);
                    paragraph.AddLast(expression);

                    foreach (Match m in expElementPattern.Matches(expression.Graph))
                    {
                        if (m.Groups.Count > 0)
                        {
                            // string m => List<Glyphs>
                            var glyphs = Script.AddGlyphs(m.Value.ToCharArray());

                            // List<Glyph> => List<Grapheme>
                            var graphemes = Orthography.AddGraphemes(glyphs);

                            // List<Grapheme> => Morpheme

                            if (string.IsNullOrEmpty(m.Groups[1].Value))
                            {
                                var morph = Text.Lexicon.Add(m.Groups[2].Value);
                                morph.GraphemeChain.Add(Lx.SegmentChain<Lx.Grapheme>.NewSegmentChain(graphemes));
                                expression.AddLast(morph);
                            }
                            else
                            {
                                expression.AddLast(Text.Paralexicon.Add(m.Groups[1].Value));
                            }


                        }
                    }

                    worker.ReportProgress(++progress, state);
                }
            }

            UpdateLocalLexicon();
        }

        private void ParseText_WorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled && e.Error == null)
            {
                MainWindow.Instance.BackgroundProgress.Value = 0;
                MainWindow.Instance.BackgroundStatus.Text = string.Empty;
                //MainWindow.Instance.UpdateWordGrid();
                MainWindow.Instance.GridModeSelector.SelectedItem = MainWindow.Instance.ShowWords;

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

            WordModel = new Lx.FiniteStateAutomoton<Lx.Grapheme>();

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
            public Lx.FiniteStateAutomoton<Lx.Grapheme> Model;
            public Lx.Lexicon Lexicon;
            public int NgramLength;
        }

        private void WordModel_DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;

            var arguments = (WordModelArguments)e.Argument;
            int progress = 0;

            foreach (var lex in arguments.Lexicon.Keys)
            {
                var graphemeChain = lex.GraphemeChain.FirstOrDefault();

                if (graphemeChain != null)
                {
                    arguments.Model.Parse(graphemeChain.ToArray(), arguments.NgramLength);
                }

                string state = lex.Graph;
                worker.ReportProgress(++progress, state);

            }
        }

        private void WordModel_WorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled && e.Error == null)
            {
                MainWindow.Instance.BackgroundProgress.Value = 0;
                MainWindow.Instance.BackgroundStatus.Text = string.Empty;

                MainWindow.Instance.GenerateWords.IsEnabled = true;
                MainWindow.Instance.UpdateNgramGrid(MainWindow.Instance.CurrentLanguage.WordModel);

                //--
                //if (MainWindow.Instance.console)
                //{
                //    MainWindow.Instance.UpdateConsole();                    
                //}
                //--
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

            foreach(var paragraph in arguments.Text.Discourse)
            {
                foreach (var exp in paragraph.Expressions)
                {
                    var morphArray = exp.Sequence.Select(m => m.Graph).ToArray();
                    arguments.Model.Parse(morphArray, arguments.ChainLength);

                    string state = exp.Graph;
                    worker.ReportProgress(++progress, state);
                }
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
                // this is problematic as it will re-add existing text lexicon
                // result is that the lexicon sums will be multiplied.

                Lexicon.Add(m.Graph.ToLower(), Text.Lexicon[m]);
            }
        }

        #region Generate

        public void GenerateWords(int number)
        {
            if (GeneratedLexicon == null)
                GeneratedLexicon = new Lx.Lexicon();

            if (WordModel == null)// || !WordModel.Transitions.Any(t => t.Chain.Contains(Lx.Node<char>.Start)))
            {
                // Word model hasn't been built yet
                // Word Model has no transitions from Start state.
                return;
            }

            var worker = new BackgroundWorker();
            worker.DoWork += GenerateWord_DoWork;
            worker.RunWorkerCompleted += GenerateWord_WorkCompleted;

            //worker.ProgressChanged += MainWindow.Instance.UpdateProgressBar;
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

                var wordString = new StringBuilder();

                foreach (var grapheme in word)
                {
                    wordString.Append(grapheme.Graph);
                }

                GeneratedLexicon.Add(wordString.ToString());
            }
        }

        private void GenerateWord_WorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled && e.Error == null)
            {
                MainWindow.Instance.UpdateGeneratedWordGrid();
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

            var random = new Random();

            var paragraph = new Lx.Discourse();
            GeneratedText.AddLast(paragraph);

            for (int l = 0; l < number; l++)
            {
                var words = TextModel.GenerateRandomChain(random);
                var line = new Lx.Expression(string.Join(" ", words));
                paragraph.AddLast(line);
            }
        }

        public void ClearGeneratedText()
        {
            if (GeneratedText != null)
                GeneratedText.Clear();
        }

        public void ClearGeneratedLexicon()
        {
            if (GeneratedLexicon != null)
                GeneratedLexicon.Clear();
        }

        #endregion
    }
}
