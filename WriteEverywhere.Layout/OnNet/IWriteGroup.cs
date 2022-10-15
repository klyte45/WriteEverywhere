namespace WriteEverywhere.Layout
{
    public interface IWriteGroup
    {
        bool HasAnyBoard();
    }
    public class WriteGroupBase : IWriteGroup
    {
        public bool HasAnyBoard() => false;
    }
}

