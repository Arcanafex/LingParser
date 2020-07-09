using System;
using System.Collections.Generic;

namespace Lx
{
    public class Language
    {
        public static Language CurrentLanguage { get; set; }

        internal readonly Guid id;
        public string Name { get; set; }
        public string Autonym { get; set; }
        public List<string> Exonyms { get; set; }

        public ConceptSet Ontology { get; set; }
        public ParadigmSet Morphology { get; set; }
        public Lexicon Lexicon { get; set; }
        public Corpus Corpus { get; set; }
    }


}

