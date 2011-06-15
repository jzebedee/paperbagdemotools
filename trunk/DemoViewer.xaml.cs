using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using SevenZip;

namespace PaperBag
{
    /// <summary>
    /// Interaction logic for DemoViewer.xaml
    /// </summary>
    public partial class DemoViewer : Window
    {
        public DemoViewer(Game game)
        {
            InitializeComponent();

            var demos = new List<string>();
            foreach (var dir in game.Paths)
            {
                var saved_demo_dir = Path.Combine(dir, DemoMover.SavedDemoDirectory);
                if (!Directory.Exists(saved_demo_dir))
                    continue;

                demos.AddRange(Directory.EnumerateFiles(saved_demo_dir, "*.?dem", SearchOption.AllDirectories));
            }

            var headers = new List<Parsers.DemoHeader>();

            foreach (var dempath in demos)
            {
                switch (Path.GetExtension(dempath))
                {
                    case DemoMover.DemoExtension:
                        headers.AddRange(demos.Select(p => Parsers.DemoHeader.Read(p)));
                        break;
                    case DemoMover.CompressedDemoExtension:
                        
                        break;
                }
            }

            headers = demos.Select(p => Parsers.DemoHeader.Read(p)).ToList();
            DataContext = headers;
        }
    }
}