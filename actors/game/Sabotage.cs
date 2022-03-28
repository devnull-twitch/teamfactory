using Godot;
using Godot.Collections;
using TeamFactory.Util.Multiplayer;
using TeamFactory.Map;
using TeamFactory.Infra;
using TeamFactory.Factory;
using TeamFactory.Input;
using TeamFactory.Items;

namespace TeamFactory.Game
{
    public enum SabotageType : int
    {
        DeleteConnection,
        TurnFactoryToBlock,
        FlipPlayerView,
        ChangeInputResource,
        DoublePowerCost,
        DisableFactory
    }

    public class Sabotage
    {
        public SabotageType Identifier;

        public string Name;

        public int PointsCost;

        public int RoundUsages;

        private GameServer gs;

        private Sabotage()
        {}

        public static Sabotage GetSabotage(SabotageType typeIdent, GameServer gs)
        {
            switch (typeIdent)
            {
                case SabotageType.DeleteConnection:
                Sabotage s = new Sabotage();
                s.Identifier = SabotageType.DeleteConnection;
                s.Name = "Delete connection";
                s.PointsCost = 50;
                s.RoundUsages = 3;
                s.gs = gs;
                return s;

                case SabotageType.TurnFactoryToBlock:
                Sabotage s1 = new Sabotage();
                s1.Identifier = SabotageType.TurnFactoryToBlock;
                s1.Name = "Turn factory to black hole";
                s1.PointsCost = 100;
                s1.RoundUsages = 1;
                s1.gs = gs;
                return s1;

                case SabotageType.FlipPlayerView:
                Sabotage s2 = new Sabotage();
                s2.Identifier = SabotageType.FlipPlayerView;
                s2.Name = "Flips camera of other player";
                s2.PointsCost = 100;
                s2.RoundUsages = 1;
                s2.gs = gs;
                return s2;

                case SabotageType.ChangeInputResource:
                Sabotage s3 = new Sabotage();
                s3.Identifier = SabotageType.ChangeInputResource;
                s3.Name = "Changes input resource";
                s3.PointsCost = 50;
                s3.RoundUsages = 2;
                s3.gs = gs;
                return s3;

                case SabotageType.DoublePowerCost:
                Sabotage s4 = new Sabotage();
                s4.Identifier = SabotageType.DoublePowerCost;
                s4.Name = "Double power cost for production for 20 seconds";
                s4.PointsCost = 150;
                s4.RoundUsages = 1;
                s4.gs = gs;
                return s4;

                case SabotageType.DisableFactory:
                Sabotage s5 = new Sabotage();
                s5.Identifier = SabotageType.DisableFactory;
                s5.Name = "Disable a factory for 30 seconds";
                s5.PointsCost = 80;
                s5.RoundUsages = 2;
                s5.gs = gs;
                return s5;

                default:
                throw new System.Exception($"unknown sabotage {typeIdent}");
            }
        }

        public void Execute(int targetNetID)
        {
            if (NetState.Mode == Mode.NET_CLIENT)
                return;

            switch (Identifier)
            {
                case SabotageType.DeleteConnection:
                Array<InfraSprite> possibleTarget = new Array<InfraSprite>();
                MapNode mapNode = gs.GetNode<MapNode>("/root/Game/GridManager");
                foreach(Node n in mapNode.GetChildren())
                {
                    if (n is InfraSprite infraNode && infraNode.OwnerID == targetNetID && infraNode.OutConnections.Count > 0)
                        possibleTarget.Add(infraNode);
                }

                if (possibleTarget.Count <= 0)
                    return;

                int targetInfraIndex = gs.Rng.RandiRange(0, possibleTarget.Count - 1);
                InfraSprite targetNode = possibleTarget[targetInfraIndex];
                int targetMapIndex = mapNode.Manager.WorldToIndex(targetNode.GlobalPosition);
                GridManager.Direction outDir = getFirstOutDirection(targetNode.OutConnections);
                mapNode.Manager.DisconnectTileResource(targetMapIndex, outDir);
                break;

                case SabotageType.TurnFactoryToBlock:
                excuteBlockingSabotage(targetNetID);
                break;

                case SabotageType.FlipPlayerView:
                executeFlipPlayerView(targetNetID);
                break;

                case SabotageType.ChangeInputResource:
                executeChangeResource(targetNetID);
                break;

                case SabotageType.DoublePowerCost:
                executeDoublePowerCost(targetNetID);
                break;

                case SabotageType.DisableFactory:
                executeDisableFactory(targetNetID);
                break;
            }
        }

        private void executeDisableFactory(int targetNetID)
        {
            Array<InfraSprite> possibleTarget = new Array<InfraSprite>();
            MapNode mapNode = gs.GetNode<MapNode>("/root/Game/GridManager");
            foreach(Node n in mapNode.GetChildren())
            {
                if (n is FactoryNode infraNode && infraNode.OwnerID == targetNetID)
                    possibleTarget.Add(infraNode);
            }

            if (possibleTarget.Count <= 0)
                return;

            int targetInfraIndex = gs.Rng.RandiRange(0, possibleTarget.Count - 1);
            InfraSprite targetNode = possibleTarget[targetInfraIndex];
            int targetMapIndex = mapNode.Manager.WorldToIndex(targetNode.GlobalPosition);
            gs.DisableNode(targetMapIndex);
            
            SceneTreeTimer timer = gs.GetTree().CreateTimer(30);
            Godot.Collections.Array args = new Godot.Collections.Array();
            args.Add(targetMapIndex);
            timer.Connect("timeout", gs, nameof(gs.EnableNode), args);
        }

        public void ReEnableNode(string nodeName)
        {
            InfraSprite node = gs.GetNode<InfraSprite>($"/root/Game/GridManager/{nodeName}");
            NetState.Rpc(node, "TriggerEnable");
        }

        private void executeChangeResource(int targetNetID)
        {
            Array<InputNode> possibleTarget = new Array<InputNode>();
            MapNode mapNode = gs.GetNode<MapNode>("/root/Game/GridManager");
            foreach(Node n in mapNode.GetChildren())
            {
                if (n is InputNode infraNode && infraNode.OwnerID == targetNetID)
                    possibleTarget.Add(infraNode);
            }

            if (possibleTarget.Count <= 0)
                return;

            int targetInfraIndex = gs.Rng.RandiRange(0, possibleTarget.Count - 1);
            InputNode targetNode = possibleTarget[targetInfraIndex];

            Array<string> unlockedItems = gs.GetPlayerUnlocks(targetNetID);
            Array<string> unlockedNonProducable = new Array<string>();
            ItemDB itemDB = GD.Load<ItemDB>("res://actors/items/ItemDB.tres");
            foreach(string itemName in unlockedItems)
            {
                ItemResource ir = itemDB.Database[itemName];
                if (ir.Requirements.Count <= 0)
                    unlockedNonProducable.Add(itemName);
            }
            unlockedNonProducable.Shuffle();
            string switchItem = unlockedNonProducable[0];
            if (switchItem == targetNode.SpawnResource.Name)
                switchItem = unlockedNonProducable[1];

            targetNode.SpawnResource = itemDB.Database[switchItem];
            NetState.Rpc(targetNode, "SpawnResourceChange", switchItem);
        }

        private void executeFlipPlayerView(int targetNetID)
        {
            gs.TriggerFlipPlayerView(targetNetID);
            SceneTreeTimer timer = gs.GetTree().CreateTimer(20);
            Godot.Collections.Array args = new Godot.Collections.Array();
            args.Add(targetNetID);
            timer.Connect("timeout", gs, nameof(gs.TriggerResetPlayerView), args);
        }

        private void executeDoublePowerCost(int targetNetID)
        {
            gs.SetPowerCostMultiplier(targetNetID, 2f);
            SceneTreeTimer timer = gs.GetTree().CreateTimer(30);
            Godot.Collections.Array args = new Godot.Collections.Array();
            args.Add(targetNetID);
            args.Add(1f);
            timer.Connect("timeout", gs, nameof(gs.SetPowerCostMultiplier), args);
        }

        private void excuteBlockingSabotage(int targetNetID)
        {
            Array<InfraSprite> possibleTarget = new Array<InfraSprite>();
            MapNode mapNode = gs.GetNode<MapNode>("/root/Game/GridManager");
            foreach(Node n in mapNode.GetChildren())
            {
                if (n is FactoryNode infraNode && infraNode.OwnerID == targetNetID)
                    possibleTarget.Add(infraNode);
            }

            if (possibleTarget.Count <= 0)
                return;

            int targetInfraIndex = gs.Rng.RandiRange(0, possibleTarget.Count - 1);
            InfraSprite targetNode = possibleTarget[targetInfraIndex];
            int targetMapIndex = mapNode.Manager.WorldToIndex(targetNode.GlobalPosition);
            mapNode.Manager.RemoveTileResource(targetMapIndex);
            mapNode.Manager.AddBlockingResource(targetMapIndex);
        }

        public GridManager.Direction getFirstOutDirection(Dictionary<GridManager.Direction, ConnectionTarget> outConns)
        {
            foreach(GridManager.Direction dir in outConns.Keys)
            {
                return dir;
            }

            throw new System.Exception("No outgoing connection found");
        }
    }
}