using Godot;
using Godot.Collections;
using TeamFactory.Util.Multiplayer;
using TeamFactory.Util.JsonMap;
using TeamFactory.Game;
using TeamFactory.Conveyor;
using TeamFactory.Infra;
using TeamFactory.Items;

namespace TeamFactory.Map 
{
    public class MapNode : Node2D
    {
        public GridManager Manager;

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

        // Server side only
        public void PlayersLoaded()
        {
            Physics2DServer.SetActive(false);
            InititMap();
        }

        [Remote]
        public void SetupManager(string filePath)
        {
            foreach(Node n in GetChildren())
            {
                n.QueueFree();
            }

            TileMap floor = GetNode<TileMap>("../Floor");
            floor.Clear();

            File testJson = new File();
            testJson.Open(filePath, File.ModeFlags.Read);

            Parser parser = new Parser(testJson.GetAsText());
            Manager = new GridManager(this, parser);
            Manager.ClientInit();
        }

        [Remote]
        public void SetupPlayerFloor(string color, int offset)
        {
            Manager.AddPlayerFloor(color, offset);
        }

        public bool NextRound()
        {
            currentRound++;
            return InititMap();
        }

        private bool InititMap()
        {
            File testJson = new File();
            Error err = testJson.Open($"res://map/round_{currentRound}.json", File.ModeFlags.Read); 
            if (err != Error.Ok)
                return false;

            NetState.Rpc(this, "SetupManager", $"res://map/round_{currentRound}.json");

            Parser parser = new Parser(testJson.GetAsText());
            MapResource template = parser.CreateMapData();
            
            GameServer gs = GetNode<GameServer>("../GameServer");
            gs.UnlockForAll(template.UnlockedItems);
            gs.TimeTillNextRound = template.Time;
            gs.ScoreLimit = template.ScoreLimit;
            gs.NewRoundStart();
            
            NetState.Rpc(GetNode<GameNode>("../"), "UpdateTimeTillNextRound", template.Time);

            if (Manager == null)
            {
                Manager = new GridManager(this, parser);
            }
            else
            {
                Manager.Cleanup();
                Manager.Parser = parser;
            }
            

            Array playerNodes = GetNode<Node>("../Players").GetChildren();
            foreach (Node2D playerNode in playerNodes)
            {
                int playerNetID = int.Parse(playerNode.Name);
                Manager.AddPlayerZone(playerNetID, NextPlayerColor);
            }

            return true;
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

        [Remote]
        public void RequestBuild(int srcIndex, GridManager.Direction rotation, InfraType.TypeIdentifier infraTypeIdent)
        {
            TileResource tileResource = new TileResource();
            tileResource.Connections = new Dictionary<GridManager.Direction, ConnectionTarget>();
            tileResource.Coords = Manager.IndexToMap(srcIndex);
            tileResource.Direction = rotation;
            tileResource.InfraTypeIdentifier = infraTypeIdent;
            tileResource.OwnerID = NetState.NetworkSenderId(this);
            
            Manager.AddTileResouce(tileResource, srcIndex);
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
        public void RelocatePlayer(int ownerID, float x, float y)
        {
            GetNode<Node2D>($"../Players/{ownerID}").Position = new Vector2(x, y);
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

            Node rawInfraNode = GD.Load<PackedScene>("res://actors/Infra/InfraNode.tscn").Instance();
            ulong objId = rawInfraNode.GetInstanceId();
            rawInfraNode.SetScript(infraType.Script);

            InfraSprite infraNode = (InfraSprite)GD.InstanceFromId(objId);
            infraNode.Texture = infraType.Texture;
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

            if (spawnResourceName == "" && infraType.isProducer)
            {
                ItemDB itemDB = GD.Load<ItemDB>("res://actors/items/ItemDB.tres");
                infraNode.SpawnResource = itemDB.Database["Ironbar"];
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