using Godot;

namespace TeamFactory.Powerplant
{
    public class PlayerPower : Resource
    {
        public int Current;

        public int MaxValue;

        public void Add(int points)
        {
            Current += points;
            if (Current > MaxValue)
                Current = MaxValue;
        }
    }
}