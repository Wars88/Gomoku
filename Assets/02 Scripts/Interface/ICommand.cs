namespace Won
{
    public interface ICommand
    {
        void Execute();
        void Undo();
    }
}
