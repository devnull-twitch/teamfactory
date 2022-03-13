using Godot;
using Godot.Collections;
using TeamFactory.Lib.Multiplayer;
using TeamFactory.Lib.JsonMap;
using TeamFactory.Game;
using TeamFactory.Conveyor;
using TeamFactory.Infra;
using TeamFactory.Items;

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

        private PackedScene packedConveyor;

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

        private Dictionary<int, InfraSprite> infraNodeCache = new Dictionary<int, InfraSprite>();

        public override void _Ready()
        {
            packedConveyor = GD.Load<PackedScene>("res://actors/conveyor/ConveyorNode.tscn");
        }

        public void PlayersLoaded()
        {
            Physics2DServer.SetActive(false);

            File testJson = new File();
            //testJson.Open($"res://map/round_{currentRound}.json", File.ModeFlags.Read);
            testJson.Open("res://map/testing.json", File.ModeFlags.Read);
            NetState.Rpc(this, "SetupManager", "res://map/testing.json");

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

        [Remote]
        public void SetupManager(string filePath)
        {
            File testJson = new File();
            testJson.Open(filePath, File.ModeFlags.Read);

            Parser parser = new Parser(testJson.GetAsText());
            Manager = new GridManager(this, parser);
            Manager.ClientInit();

            MapResource template = parser.CreateMapData();
            UnlockedItems = template.UnlockedItems;
        }

        [Remote]
        public void SetupPlayerFloor(string color, int offset)
        {
            Manager.AddPlayerFloor(color, offset);
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

        [Remote]
        public void RequestConnection(int srcIndex, int targetIndex, GridManager.Direction outputDir)
        {
            Manager.ConnectTileResource(srcIndex, targetIndex, outputDir);
        }

        [Remote]
        public void RequestDisconnect(int srcIndex, GridManager.Direction outDirection)
        {
            Manager.DisconnectTileResource(srcIndex, outDirection);
        }

        [RemoteSync]
        public void CreateConveyorNode(string nodeName, int conveyorIndex, GridManager.Direction inputDir, GridManager.Direction outputDir)
        {
            ConveyorNode conveyorInstance = packedConveyor.Instance<ConveyorNode>();
            conveyorInstance.Name = nodeName;
            conveyorInstance.Position = Manager.IndexToWorld(conveyorIndex);
            conveyorInstance.InputDir = inputDir;
            conveyorInstance.OutputDir = outputDir;

            AddChild(conveyorInstance);
        }

        [RemoteSync]
        public void CreateInfraNode(
            string nodeName,
            InfraType.TypeIdentifier infraTypeIdent,
            int index,
            GridManager.Direction rotation,
            string spawnResourceName,
            int ownerID
        ) 
        {
            InfraType infraType = InfraType.GetByIdentifier(infraTypeIdent);

            InfraSprite infraNode = infraType.Scene.Instance<InfraSprite>();
            infraNode.Position = Manager.IndexToWorld(index);
            infraNode.RotateFromDirection(rotation);
            infraNode.GridManager = Manager;
            infraNode.Name = nodeName;
            infraNode.Type = infraType;
            infraNode.OutConnections = new Dictionary<GridManager.Direction, ConnectionTarget>();
            infraNode.InConnections = new Dictionary<GridManager.Direction, ConnectionTarget>();
            infraNode.OwnerID = ownerID;

            if (spawnResourceName != "")
            {
                ItemDB itemDB = GD.Load<ItemDB>("res://actors/items/ItemDB.tres");
                infraNode.SpawnResource = itemDB.Database[spawnResourceName];
                // TODO add spawn timer to item resource
                infraNode.SpawnInterval = 1f;
            }

            infraNodeCache[index] = infraNode;

            AddChild(infraNode);
        }

        public bool IndexHasNode(int index)
        {
            return infraNodeCache.ContainsKey(index);
        }

        [RemoteSync]
        public void SaveConnection(int outIndex, GridManager.Direction outDir, int inIndex, GridManager.Direction inDir)
        {
            string outNodeName = $"InfraNode_{outIndex}";
            Vector2 outMapCoords = Manager.IndexToMap(outIndex);

            string inNodeName = $"InfraNode_{inIndex}";
            Vector2 inMapCoords = Manager.IndexToMap(inIndex);

            GetNode<InfraSprite>(outNodeName).OutConnections[outDir] = new ConnectionTarget(inMapCoords, inDir);
            GetNode<InfraSprite>(inNodeName).InConnections[inDir] = new ConnectionTarget(outMapCoords, outDir);
        }

        // Calls from server to ask if client is done with setup for player
        [Remote]
        public void AckReady(int playerNetID)
        {
            NetState.RpcId(this, 1, "RequestAckConfirm", playerNetID);
        }

        // Calls from client confirming they are done with player
        [Remote]
        public void RequestAckConfirm(int playerNetID)
        {
            Manager.IncAckPlayerReady(playerNetID);
        }
    }
}