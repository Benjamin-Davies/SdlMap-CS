namespace SdlMapCS
{
    public interface TileLoader
    {
        int Active { get; }
        int Work();
        void Queue(Tile tile);
    }
}