using Godot;
using Godot.Collections;
using TeamFactory.Util.Multiplayer;
using TeamFactory.Map;
using TeamFactory.Infra;
using TeamFactory.Factory;

namespace TeamFactory.Game
{
    public enum SabotageType : int
    {
        DeleteConnection,
        TurnFactoryToBlock
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
            }
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