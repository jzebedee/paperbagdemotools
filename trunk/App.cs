using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace PaperBag
{
    public class App : Application
    {
        [STAThread]
        static void Main()
        {
            var app = new App();
            app.DispatcherUnhandledException += (sender, e) => MessageBox.Show(e.ToString());
            try
            {
                app.Run(new MainWindow());
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
    }
}
