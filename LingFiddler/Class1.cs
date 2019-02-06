using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LingFiddler
{
    public class Word
    {
        private static readonly char[] filterChars = { '_', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

        private static List<Word> words;
        public static List<Word> Words
        {
            get
            {
                if (words == null)
                {
                    words = new List<Word>();
                }

                return words;
            }
            set
            {
                words = value;
            }
        }

        public static void Add(string word)
        {
            if (String.IsNullOrEmpty(word))
                return;

            word = word.ToLower().Trim(Word.filterChars);

            if (word == String.Empty)
                return;

            var thisWord = Words.FirstOrDefault(w => w.Graph == word);

            if (thisWord == null)
            {
                thisWord = new Word(word)
                {
                    Count = 1
                };
                Words.Add(thisWord);
            }
            else
            {
                thisWord.Count++;
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
        public int Count { get; set; }

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


}
