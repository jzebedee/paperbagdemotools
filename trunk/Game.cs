using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PaperBag
{
    [Serializable]
    public class Game
    {
        [NonSerialized]
        const string
            ScriptFilename = "paperbag.cfg",
            execLine = "exec " + ScriptFilename;

        DemoMover Mover
        {
            get
            {
                return DemoMover.Instance;
            }
        }

        public Game()
        {
            Mover.PropertyChanged += Parent_PropertyChanged;
        }
        ~Game()
        {
            Mover.PropertyChanged -= Parent_PropertyChanged;
        }

        void Parent_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentGameMap" && Mover.CurrentGameMap != null)
                Apply();
        }

        public string Name { get; set; }
        public IEnumerable<string> Paths { get; set; }

        public bool Compress { get; set; }

        bool _enabled = true;

        public bool Enabled
        {
            get
            {
                return _enabled;
            }
            set
            {
                _enabled = value;
                Apply();
            }
        }

        string
            _bind_AddMarker = null,
            _bind_StopDemo = null,
            _bind_StartDemo = null;

        public string Bind_AddMarker
        {
            get
            {
                return _bind_AddMarker;
            }
            set
            {
                _bind_AddMarker = value;
                UpdateConfig(true);
            }
        }

        public string Bind_StopDemo
        {
            get
            {
                return _bind_StopDemo;
            }
            set
            {
                _bind_StopDemo = value;
                UpdateConfig(true);
            }
        }

        public string Bind_StartDemo
        {
            get
            {
                return _bind_StartDemo;
            }
            set
            {
                _bind_StartDemo = value;
                UpdateConfig(true);
            }
        }

        public void Apply()
        {
            if (Paths == null || !Enabled)
                return;

            UpdateConfig();
            MoveLooseDemos();

            foreach (var path in Paths)
                if (!Mover.Watches.ContainsKey(path))
                {
                    var watcher = new FileSystemWatcher(path, "*.dem") { EnableRaisingEvents = Enabled };
                    watcher.Changed += (sender, e) => Mover.DemoFound(sender, e, () => Compress);

                    Mover.Watches.Add(path, watcher);
                }
                else
                    Mover.Watches[path].EnableRaisingEvents = Enabled;
        }

        public void MoveLooseDemos()
        {
            if (Paths == null)
                return;

            foreach (var path in Paths)
                foreach (var looseDemo in Directory.EnumerateFiles(path, "*.dem", SearchOption.TopDirectoryOnly))
                    Mover.ProcessDemo(looseDemo, Compress);
        }

        public void UpdateConfig(bool Force = false)
        {
            if (Paths == null)
                return;

            foreach (var path in Paths)
            {
                var cfg_dir = Path.Combine(path, "cfg");

                var script_path = Path.Combine(cfg_dir, ScriptFilename);
                var autoexec_path = Path.Combine(cfg_dir, "autoexec.cfg");

                if (!File.Exists(script_path) || Force)
                    File.WriteAllText(script_path, GetScriptContents());

                if (File.Exists(autoexec_path))
                {
                    bool foundExec = false;
                    using (var reader = new StreamReader(autoexec_path))
                    {
                        string line;
                        while (!reader.EndOfStream)
                            if ((line = reader.ReadLine()).Contains(execLine))
                            {
                                foundExec = true;
                                break;
                            }
                    }

                    if (!foundExec)
                        using (var appender = File.AppendText(autoexec_path))
                        {
                            appender.WriteLine();
                            appender.WriteLine(execLine);
                        }
                }
            }
        }

        string GetScriptContents(int Count = 100)
        {
            var SB = new StringBuilder();

            SB.AppendLine("alias pb_record pb_record_0");
            SB.AppendLine();
            for (int i = 0; i < Count; i++)
                SB.AppendFormat("alias pb_record_{0} \"stop; record {2}{0}; alias pb_record pb_record_{1}\"", i, i + 1, DemoMover.AutoDemoPrefix).AppendLine();

            SB.AppendFormat("alias pb_record_{0} \"stop\"", Count).AppendLine();
            SB.AppendLine();

            if (!string.IsNullOrEmpty(Bind_AddMarker))
                SB.AppendFormat("bind {0} {1}", Bind_AddMarker, "demo_pause").AppendLine();

            if (!string.IsNullOrEmpty(Bind_StartDemo))
                SB.AppendFormat("bind {0} {1}", Bind_StartDemo, "pb_record").AppendLine();

            if (!string.IsNullOrEmpty(Bind_StopDemo))
                SB.AppendFormat("bind {0} {1}", Bind_StopDemo, "stop").AppendLine();

            return SB.ToString();
        }
    }
}