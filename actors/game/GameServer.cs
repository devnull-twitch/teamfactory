using Godot;
using Godot.Collections;
using TeamFactory.Lib.Multiplayer;

namespace TeamFactory.Game
{
    public class GameServer : Node
    {
        public GameNode Node;

        protected Dictionary<int, int> UserPoints = new Dictionary<int, int>();

        public void AddPoints(int ownerID, int points)
        {
            if (!UserPoints.ContainsKey(ownerID))
            {
                UserPoints[ownerID] = 0;
            }

            UserPoints[ownerID] = UserPoints[ownerID] + points;
            NetState.Rpc(Node, "SetPoints", ownerID, UserPoints[ownerID]);
        }
    }
}