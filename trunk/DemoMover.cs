using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Win32;
using PaperBag.Parsers;

namespace PaperBag
{
    public class DemoMover : INotifyPropertyChanged, IDisposable
    {
        public readonly string SteamPath;

        internal const string
            GameMapFilename = "Games.dat",
            SavedDemoDirectory = "saved_demos",
            DemoExtension = ".dem",
            CompressedDemoExtension = ".cdem",
            AutoDemoPrefix = "pb_demo_";

        public readonly Dictionary<string, FileSystemWatcher> Watches = new Dictionary<string, FileSystemWatcher>();

        public static DemoMover Instance = new DemoMover();

        private DemoMover()
        {
            using (var steamKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\VALVE\\STEAM"))
                SteamPath = Path.Combine((string)steamKey.GetValue("STEAMPATH"), "steamapps");

            Trace.Assert(Directory.Exists(SteamPath), "Could not load Steam installation path from registry");
        }
        ~DemoMover()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (CurrentGameMap != null)
                    CurrentGameMap.Save(GameMapFilename);
            }
        }

        public void DemoFound(object sender, FileSystemEventArgs e, Func<bool> shouldCompress)
        {
            if (e.ChangeType.HasFlag(WatcherChangeTypes.Created | WatcherChangeTypes.Changed))
                return;

            ProcessDemo(e.FullPath, shouldCompress());
        }

        public void ProcessDemo(string demoPath, bool compress)
        {
            Task.Factory.StartNew(() => ProcessDemoInternal(demoPath, compress));
        }
        private void ProcessDemoInternal(string demoPath, bool compress)
        {
            var parent_dir = Path.GetDirectoryName(demoPath);
            var demo_dir = Path.Combine(parent_dir, SavedDemoDirectory);

            Directory.CreateDirectory(demo_dir);

            var fileInfo = new FileInfo(demoPath);
            var fileName = fileInfo.Name;

            var demo_date = fileInfo.CreationTime;

            var dated_demo_dir = Path.Combine(demo_dir, demo_date.ToString("yyyy-MM-dd"));
            Directory.CreateDirectory(dated_demo_dir);

            var DEMInfo = DemoHeader.Read(demoPath);

            bool manualDemo = !fileName.Contains(AutoDemoPrefix);
            var newPath = Path.Combine(dated_demo_dir,
                string.Format("{0},{1}{2}",
                DEMInfo.MapName,
                manualDemo ? fileName + "," : "",
                demo_date.ToString("HH-mm-ss")));

            var extension = compress ? CompressedDemoExtension : DemoExtension;
            if (File.Exists(newPath + extension))
                newPath += demo_date.ToString("-ffff");
            newPath += extension;

            if (compress)
            {
                FileCompression.Compress(newPath, demoPath);
                File.Delete(demoPath);
            }
            else
                File.Move(demoPath, newPath);
        }

        public void RecreateMap()
        {
            GameMap
                oldMap = CurrentGameMap,
                newMap = new GameMap() { Map = BuildGameMap() };

            if (oldMap.Map != null)
            {
                foreach (var game in oldMap.Map)
                {
                    var newGame = newMap.Map.SingleOrDefault(g => g.Name.Equals(game.Name, StringComparison.InvariantCultureIgnoreCase));
                    if (newGame != null)
                    {
                        newGame.Bind_AddMarker = game.Bind_AddMarker;
                        newGame.Bind_StartDemo = game.Bind_StartDemo;
                        newGame.Bind_StopDemo = game.Bind_StopDemo;
                        newGame.Compress = game.Compress;
                        newGame.Enabled = game.Enabled;
                    }
                }
            }

            File.Delete(GameMapFilename);
            CurrentGameMap = newMap;
        }

        public IEnumerable<string> GameDirs
        {
            get
            {
                return from d in Directory.EnumerateDirectories(SteamPath, "cfg", SearchOption.AllDirectories)
                       select Path.GetDirectoryName(d);
            }
        }

        private GameMap _loaded_GameMap = null;
        public GameMap CurrentGameMap
        {
            get
            {
                if (_loaded_GameMap != null)
                    return _loaded_GameMap;

                if (File.Exists(GameMapFilename))
                    CurrentGameMap = GameMap.Load(GameMapFilename);
                else
                {
                    var newMap = new GameMap() { Map = BuildGameMap() };
                    CurrentGameMap = newMap;
                }

                return _loaded_GameMap;
            }
            set
            {
                if (PropertyChanged != null)
                    if (_loaded_GameMap != value)
                    {
                        _loaded_GameMap = value;
                        PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs("CurrentGameMap"));
                    }
            }
        }

        IList<Game> BuildGameMap()
        {
            var gameDict = new Dictionary<string, HashSet<string>>();

            var gameInfoPaths = new List<string>();
            foreach (var dir in GameDirs)
                gameInfoPaths.AddRange(Directory.EnumerateFiles(dir, "GameInfo.txt", SearchOption.TopDirectoryOnly));

            foreach (var gameInfo in gameInfoPaths.Select(path => new GameInfo(path)))
                if (gameInfo.AppID > 0 && !string.IsNullOrWhiteSpace(gameInfo.Game))
                {
                    if (!gameDict.ContainsKey(gameInfo.Game))
                        gameDict.Add(gameInfo.Game, new HashSet<string>());
                    gameDict[gameInfo.Game].Add(Path.GetDirectoryName(gameInfo.Path));
                }

            return gameDict.Select(kvp => new Game { Name = kvp.Key, Paths = kvp.Value.ToList() }).ToList();
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    }
}