using TeamFactory.Items;

namespace TeamFactory.Infra
{
    public interface IItemReceiver
    {
        void ItemArrived(ItemNode itemNode);
    }
}