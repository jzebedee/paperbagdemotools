using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace PaperBag
{
    [Serializable]
    public class GameMap : IDeserializationCallback
    {
        public ObservableCollection<Game> Map { get; private set; }
        public GameMap(IEnumerable<Game> games)
        {
            Map = new ObservableCollection<Game>(games);
        }

        public void Save(string path)
        {
            using (var outstream = File.OpenWrite(path))
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(outstream, this);
            }
        }

        public static GameMap Load(string path)
        {
            using (var instream = File.OpenRead(path))
            {
                var formatter = new BinaryFormatter();
                return formatter.Deserialize(instream) as GameMap;
            }
        }

        public void OnDeserialization(object sender)
        {
            if (Map != null)
            {
                foreach (var game in Map)
                {
                    if (game != null)
                        game.Apply();
                }
            }
        }
    }
}