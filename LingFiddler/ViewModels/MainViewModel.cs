using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LingFiddler
{
    public class MainViewModel
    {
        internal LingMachine CurrentLanguage { get; set; }
        private void InitializeApp()
        {
            CurrentLanguage = new LingMachine();
            CurrentCharSet = new HashSet<char>();
        }
    }
}
