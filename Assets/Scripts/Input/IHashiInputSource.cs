namespace ChopChop.Input
{
    public interface IHashiInputSource
    {
        bool IsRunning { get; }
        HashiInputFrame CurrentFrame { get; }
        void StartInput();
        void StopInput();
        void Tick(float deltaTime);
    }
}
