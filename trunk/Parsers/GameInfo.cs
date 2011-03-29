using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace PaperBag.Parsers
{
    //Someday, we might want to parse out more from GameInfo.txt
    public class GameInfo
    {
        static Regex
            AppIDRegex = new Regex("(\"?)SteamAppId(\"?)(\\s+)(\"?)([0-9]+)(\"?)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            GameRegex = new Regex("(\"?)game(\"?)(\\s+)\\\"(.+?)\\\"", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public GameInfo(string file)
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

            this.AppID = appID;
            this.Game = game;
            this.Path = file;
        }

        public readonly int AppID;
        public readonly string
            Game,
            Path;
    }
}
