using System;
using System.Collections.Generic;
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
    /// Interaction logic for DiscourseEntry.xaml
    /// </summary>
    public partial class DiscourseEntry : UserControl
    {
        private DiscourseEntryViewModel m_view;

        public DiscourseEntry()
        {
            m_view = new DiscourseEntryViewModel();
            DataContext = m_view;
            InitializeComponent();
        }
    }
}
