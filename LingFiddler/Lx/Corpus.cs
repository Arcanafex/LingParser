using System.Collections.Generic;

namespace Lx
{
    /// <summary>
    /// A collection of Texts.
    /// </summary>
    public class Corpus : HashSet<Text>
    {
        //a set of Texts
        public string Title { get; set; }
        public string Description { get; set; }
    }


}

