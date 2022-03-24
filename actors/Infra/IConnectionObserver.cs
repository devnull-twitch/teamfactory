namespace TeamFactory.Infra
{
    public interface IConnectionObserver
    {
        void NewOutConnection();

        void NewInConnection();
    }
}