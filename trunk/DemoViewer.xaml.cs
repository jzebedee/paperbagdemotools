﻿using System.Collections.Generic;
using System.IO;
using System.Windows;

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