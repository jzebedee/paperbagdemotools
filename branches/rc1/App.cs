using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace PaperBag
{
    public class App : Application
    {
        const string
            LogFile = "debug.log",
            LogName = "DebugLogFile";

        [STAThread]
        static void Main()
        {
            File.Delete(LogFile);

            Trace.Listeners.Insert(0, new TextWriterTraceListener(LogFile, LogName));
            Trace.AutoFlush = true;

            var app = new App();
            app.DispatcherUnhandledException += (sender, e) => Trace.WriteLine(e);
            app.Run(new MainWindow());
        }
    }
}