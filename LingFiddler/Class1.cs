using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LingFiddler
{
    public class Word
    {
        public static char[] FilterChars { get; set; } = { '_', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

        private static HashSet<Word> words;
        public static HashSet<Word> Words
        {
            get
            {
                if (words == null)
                {
                    words = new HashSet<Word>();
                }

                return words;
            }
            set
            {
                words = value;
            }
        }

        public static void Add(string word, float weight = 1.0f)
        {
            if (String.IsNullOrEmpty(word))
                return;

            word = word.ToLower().Trim(Word.FilterChars);

            if (word == String.Empty)
                return;

            foreach (var morph in word.Split(Word.FilterChars, StringSplitOptions.RemoveEmptyEntries))
            {
                var thisWord = Words.FirstOrDefault(w => w.Graph == morph);

                if (thisWord == null)
                {
                    thisWord = new Word(morph)
                    {
                        Frequency = weight
                    };

                    Words.Add(thisWord);
                }
                else
                {
                    thisWord.Frequency += weight;
                }
            }
        }

        public static void Remove(Word word)
        {
            Words.Remove(word);
        }

        public static void Clear()
        {
            Words.Clear();
        }

        public string Graph { get; set; }
        public int Length { get { return Graph.Length; } }
        public float Frequency { get; set; }

        public Word(string graph)
        {
            this.Graph = graph;
        }

        public override bool Equals(object obj)
        {
            var other = obj as Word;
            return this.Graph.Equals(other.Graph);
        }

        public override string ToString()
        {
            return this.Graph;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class Grapheme
    {
        private static HashSet<char> graphemes;
        public static HashSet<char> Graphemes
        {
            get
            {
                if (graphemes == null)
                    graphemes = new HashSet<char>();

                return graphemes;
            }
            set
            {
                graphemes = value;
            }
        }

        public string Glyph { get; set; }
        public float Frequency { get; set; }
    }

    internal class Lexicon
    {

    }
}
