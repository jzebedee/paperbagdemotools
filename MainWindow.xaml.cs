using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using SevenZip;

namespace PaperBag
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = DemoMover.Instance;

            Tray.Enable(this);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MinWidth = ActualWidth;
            MinHeight = ActualHeight;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            DemoMover.Instance.Dispose();
        }

        private void Button_ReloadGames_Click(object sender, RoutedEventArgs e)
        {
            DemoMover.Instance.RecreateMap();
        }

        private void Button_ViewDemos_Click(object sender, RoutedEventArgs e)
        {
            var game = GameList.SelectedItem as Game;
            if (game == null)
                return;

            new DemoViewer(game).ShowDialog();
        }
    }
}