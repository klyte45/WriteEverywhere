namespace WriteEverywhere.Xml
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

