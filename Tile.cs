using System;

using static SDL2.SDL;
using static SDL2.SDL_image;

namespace SdlMapCS
{
    public enum TileState
    {
        Empty,
        Rough,
        Queued,
        Loaded
    }

    public class Tile
    {
        public const int TileSize = 256;

        private IntPtr Surface;

        public TileState State = TileState.Empty;
        public int Zoom, X, Y;

        public Tile(int x, int y, int zoom)
        {
            X = x;
            Y = y;
            Zoom = zoom;
        }

        public bool Loaded => Surface != IntPtr.Zero;

        public void Queue()
        {
            State = TileState.Queued;
        }

        public void Load(IntPtr surface)
        {
            Surface = surface;
            State = TileState.Loaded;
        }

        public string Url =>
            $"http://mts0.google.com/vt/hl=en&src=api&x={X}&s=&y={Y}&z={Zoom}";

        public void Render(IntPtr screen, int offsetX, int offsetY)
        {
            var dest = new SDL_Rect
            {
                x = X * TileSize - offsetX,
                y = Y * TileSize - offsetY,
                w = TileSize,
                h = TileSize
            };
            while (dest.x <= -TileSize)
                dest.x += (1 << Zoom) * TileSize;
            if (Surface != IntPtr.Zero)
                SDL_BlitSurface(Surface, IntPtr.Zero, screen, ref dest);
            else
                SDL_FillRect(screen, ref dest, 0xFFFFFF);
        }
    }
}
