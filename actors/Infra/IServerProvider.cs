using Godot;

namespace TeamFactory.Infra
{
    public interface IServerProvider 
    {
        Node ServerNode { get; }
    }
}