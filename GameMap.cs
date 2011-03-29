using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace PaperBag
{
    [XmlRoot("GameMap")]
    public class GameMap
    {
        [XmlArray]
        public ObservableCollection<Game> Games { get; set; }
    }
}
