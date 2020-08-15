using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace LingFiddler
{
    public partial class MainWindow : Window
    {
        internal static MainWindow Instance { get; private set; }
        
        internal MainViewModel m_viewModel { get; set; }

        public MainWindow()
        {
            Instance = this;
            InitializeComponent();

            m_viewModel = new MainViewModel();
            DataContext = m_viewModel;
        }
    }
}
