using Godot;
using System;
using System.Reflection;

namespace TeamFactory.Util.Multiplayer
{
    public enum Mode
    {
        NET_SERVER = 1,
        NET_CLIENT = 2,
        LOCAL = 3
    }
    
    public class NetState
    {
        public static Mode Mode = Mode.NET_CLIENT;

        public static void Rpc(Node node, string method, params object[] args)
        {
            if (NetState.Mode == Mode.LOCAL)
            {
                Type t = node.GetType();
                MethodInfo mi = t.GetMethod(method);
                if (mi == null)
                {
                    throw new Exception($"Method {method} on {t} does not exist");
                }

                mi.Invoke(node, args);
                return;
            }

            node.Rpc(method, args);
        }

        public static void RpcId(Node node, int id ,string method, params object[] args)
        {
            if (NetState.Mode == Mode.LOCAL)
            {
                Type t = node.GetType();
                MethodInfo mi = t.GetMethod(method);
                if (mi == null)
                {
                    throw new Exception($"Method {method} on {t} does not exist");
                }

                mi.Invoke(node, args);
                return;
            }

            node.RpcId(id, method, args);
        }

        public static int NetworkId(Node node)
        {
            if (node.GetTree().NetworkPeer == null)
            {
                return 1337;
            }

            return node.GetTree().GetNetworkUniqueId();
        }

        public static int NetworkSenderId(Node node)
        {
            if (node.GetTree().NetworkPeer == null)
            {
                return 1337;
            }

            return node.GetTree().GetRpcSenderId();
        }
    }
}