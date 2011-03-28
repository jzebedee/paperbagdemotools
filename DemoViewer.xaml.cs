using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SevenZip;

namespace PaperBag
{
    /// <summary>
    /// Interaction logic for DemoViewer.xaml
    /// </summary>
    public partial class DemoViewer : Window
    {
        public DemoViewer()
        {
            InitializeComponent();

            var mover = DemoMover.Instance;

            var demos = new List<string>();

            foreach (var dir in mover.GameDirs)
            {
                var saved_demo_dir = Path.Combine(dir, DemoMover.SavedDemoDirectory);
                if (!Directory.Exists(saved_demo_dir))
                    continue;

                demos.AddRange(Directory.EnumerateFiles(saved_demo_dir, "*.?dem", SearchOption.AllDirectories));
            }

            DataContext = demos;
        }
    }
}