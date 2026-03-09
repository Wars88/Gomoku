namespace Won
{
    public class StoneCommand : ICommand
    {
        private readonly IGomokuModel _model;
        public int X { get; private set; }
        public int Y { get; private set; }

        public StoneCommand(IGomokuModel model, int x, int y)
        {
            _model = model;
            X = x;
            Y = y;
        }

        public void Execute()
        {
            _model.PlaceStone(X, Y);
        }

        public void Undo()
        {
            _model.UndoStone(_x, _y);
        }
    }
}