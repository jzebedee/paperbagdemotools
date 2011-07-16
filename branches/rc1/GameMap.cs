using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using ProtoBuf;

namespace PaperBag
{
    [ProtoContract]
    public class GameMap
    {
        [ProtoMember(1)]
        public IList<Game> Map { get; set; }

        public void Save(string path)
        {
            using (var outstream = File.OpenWrite(path))
            {
                Serializer.Serialize(outstream, this);
            }
        }

        public static GameMap Load(string path)
        {
            using (var instream = File.OpenRead(path))
            {
                return Serializer.Deserialize<GameMap>(instream);
            }
        }
    }
}