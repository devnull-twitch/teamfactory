using Godot;
using Godot.Collections;
using TeamFactory.Lib.Multiplayer;
using TeamFactory.Lib.JsonMap;
using TeamFactory.Game;

namespace TeamFactory.Map 
{
    public class MapNode : Node2D
    {
        public GridManager Manager;

        public Array<string> UnlockedItems;

        private string[] availableColors = new string[] {
            "#FF0000",
            "#00FF00",
            "#0000FF"
        };

        private int currentRound = 1;

        private int nextColorIndex;

        public string NextPlayerColor
        {
            get {
                string c = availableColors[nextColorIndex];
                nextColorIndex++;
                if (nextColorIndex >= availableColors.Length)
                {
                    nextColorIndex = 0;
                }

                return c;
            }
        }

        public override void _Ready()
        {
            File testJson = new File();
            testJson.Open($"res://map/round_{currentRound}.json", File.ModeFlags.Read);

            Parser parser = new Parser(testJson.GetAsText());

            MapResource template = parser.CreateMapData();
            UnlockedItems = template.UnlockedItems;
            GetNode<GameNode>("../").TimeTillNextRound = template.Time;
            GetNode<GameServer>("../GameServer").TimeTillNextRound = template.Time;

            Manager = new GridManager(this, parser);

            foreach (int playerNetID in GetNode<GameServer>("../GameServer").GetPlayerIDs())
            {
                Manager.AddPlayerZone(playerNetID, NextPlayerColor);
            }
        }

        [RemoteSync]
        public void NextRound()
        {
            currentRound++;
            File testJson = new File();
            testJson.Open($"res://map/round_{currentRound}.json", File.ModeFlags.Read);

            Parser parser = new Parser(testJson.GetAsText());

            MapResource template = parser.CreateMapData();
            UnlockedItems = template.UnlockedItems;
            GetNode<GameNode>("../").TimeTillNextRound = template.Time;
            GetNode<GameServer>("../GameServer").TimeTillNextRound = template.Time;

            Manager.Cleanup();
            Manager.Parser = parser;

            Array playerNodes = GetNode<Node>("../Players").GetChildren();
            foreach (Node2D playerNode in playerNodes)
            {
                int playerNetID = int.Parse(playerNode.Name);
                Manager.AddPlayerZone(playerNetID, NextPlayerColor);
            }
        }

        public void CreatePlayerZone(int netID)
        {
            Manager.AddPlayerZone(netID, NextPlayerColor);
        }
    }
}