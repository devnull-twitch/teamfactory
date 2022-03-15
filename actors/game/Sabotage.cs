using Godot;
using Godot.Collections;
using TeamFactory.Util.Multiplayer;
using TeamFactory.Map;
using TeamFactory.Infra;

namespace TeamFactory.Game
{
    public enum SabotageType : int
    {
        DeleteConnection
    }

    public class Sabotage
    {
        public SabotageType Identifier;

        public string Name;

        public int PointsCost;

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
                s.gs = gs;
                return s;

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
            }
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