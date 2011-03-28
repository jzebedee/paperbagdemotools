using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using System.Xml;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Threading.Tasks;

namespace PaperBag
{
    public class DemoMover : INotifyPropertyChanged
    {
        public readonly string SteamPath;

        static Regex
            AppIDRegex = new Regex("(\"?)SteamAppId(\"?)(\\s+)(\"?)([0-9]+)(\"?)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            GameRegex = new Regex("(\"?)game(\"?)(\\s+)\\\"(.+?)\\\"", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        internal const string
            GameMapFilename = "Games.xml",
            SavedDemoDirectory = "saved_demos",
            DemoExtension = ".dem",
            CompressedDemoExtension = ".cdem",
            AutoDemoPrefix = "pb_demo_";

        public readonly Dictionary<string, FileSystemWatcher> Watches = new Dictionary<string, FileSystemWatcher>();

        #region Singleton
        static object _instance_lock = new object();
        static DemoMover _instance = null;
        public static DemoMover Instance
        {
            get
            {
                lock (_instance_lock)
                {
                    if (_instance == null)
                        _instance = new DemoMover();

                    return _instance;
                }
            }
        }
        #endregion

        private DemoMover()
        {
            using (var steamKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\VALVE\\STEAM"))
                SteamPath = Path.Combine((string)steamKey.GetValue("STEAMPATH"), "steamapps");
        }

        public void DemoFound(object sender, FileSystemEventArgs e, Func<bool> shouldCompress)
        {
            if (e.ChangeType.HasFlag(WatcherChangeTypes.Created | WatcherChangeTypes.Changed))
                return;

            ProcessDemo(e.FullPath, shouldCompress());
        }

        public void ProcessDemo(string demoPath, bool compress)
        {
            Task.Factory.StartNew(() =>
            {
                var parent_dir = Path.GetDirectoryName(demoPath);
                var demo_dir = Path.Combine(parent_dir, SavedDemoDirectory);

                if (!Directory.Exists(demo_dir))
                    Directory.CreateDirectory(demo_dir);

                var fileInfo = new FileInfo(demoPath);
                var fileName = fileInfo.Name;

                var demo_date = fileInfo.CreationTime;

                var dated_demo_dir = Path.Combine(demo_dir, demo_date.ToString("yyyy-MM-dd"));
                if (!Directory.Exists(dated_demo_dir))
                    Directory.CreateDirectory(dated_demo_dir);

                var DEMInfo = DEM.ReadDemoHeader(demoPath);

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
            });
        }

        public void RecreateMap()
        {
            File.Delete(GameMapFilename);
            _game_dirs = null;
            CurrentGameMap = null;
        }

        GameMap Load()
        {
            var XS = new XmlSerializer(typeof(GameMap));
            using (var mapReader = new StreamReader(GameMapFilename))
                return XS.Deserialize(mapReader) as GameMap;
        }

        public void Save()
        {
            var XS = new XmlSerializer(typeof(GameMap));
            using (var mapWriter = new StreamWriter(GameMapFilename))
                XS.Serialize(mapWriter, _loaded_GameMap);
        }

        List<string> _game_dirs = null;
        public List<string> GameDirs
        {
            get
            {
                if (_game_dirs == null)
                    _game_dirs = (from d in Directory.EnumerateDirectories(SteamPath, "cfg", SearchOption.AllDirectories)
                                  select Path.GetDirectoryName(d)).ToList();

                return _game_dirs;
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
                    CurrentGameMap = Load();
                else
                {
                    List<string> gameInfos = new List<string>();

                    foreach (var dir in GameDirs)
                        gameInfos.AddRange(Directory.EnumerateFiles(dir, "GameInfo.txt", SearchOption.TopDirectoryOnly));

                    var gameDict = new Dictionary<string, SerializableDictionary<string, int>>();
                    foreach (var file in gameInfos)
                    {
                        int appID = 0;
                        string game = "";

                        Match appIDMatch, gameMatch;

                        using (var reader = new StreamReader(file))
                            while (!reader.EndOfStream && (appID == 0 || game == ""))
                            {
                                var line = reader.ReadLine();
                                if (appID == 0 && (appIDMatch = AppIDRegex.Match(line)).Success)
                                    appID = int.Parse(appIDMatch.Groups[5].Value);
                                if (game == "" && (gameMatch = GameRegex.Match(line)).Success)
                                    game = gameMatch.Groups[4].Value;
                            }

                        if (appID > 0 && !string.IsNullOrWhiteSpace(game))
                        {
                            if (!gameDict.ContainsKey(game))
                                gameDict.Add(game, new SerializableDictionary<string, int>());
                            gameDict[game].Add(Path.GetDirectoryName(file), appID);
                        }
                    }

                    CurrentGameMap = new GameMap { Games = new ObservableCollection<Game>(gameDict.Select((kvp) => new Game { Name = kvp.Key, Paths = kvp.Value })) };
                    Save();
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

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    }
}