using System;

namespace SdlMapCS
{
    public class MapView
    {
        private int Width, Height;
        private int OffsetX, OffsetY;

        public TileCollection Tiles = new TileCollection(0, 0, 0, 0, 0);
        public int Zoom;

        public MapView(int width, int height, int zoom)
        {
            OffsetX = 0;
            OffsetY = 0;
            Zoom = zoom;
        }

        public void CenterCoords(double lat, double lng)
        {
            double x = (lng + 180) / 360;
            double y = (1 - Math.Log(
                    Math.Tan(lat * Math.PI / 180)
                    + 1 / Math.Cos(lat * Math.PI / 180)
                ) / Math.PI) / 2;
            OffsetX = (int)(x * (Tile.TileSize << Zoom) - Width / 2);
            OffsetY = (int)(y * (Tile.TileSize << Zoom) - Height / 2);
        }

        public void Render(IntPtr surface)
        {
            Tiles.Render(surface, OffsetX, OffsetY);
        }

        public void MoveBy(int dx, int dy)
        {
            OffsetX += dx;
            OffsetY += dy;
        }

        public void ZoomAt(int x, int y)
        {
            if (Zoom < 22)
            {
                Zoom++;
                OffsetX = OffsetX * 2 + x;
                OffsetY = OffsetY * 2 + y;
            }
        }

        public void ZoomIn()
        {
            ZoomAt(Width / 2, Height / 2);
        }

        public void ZoomOut()
        {
            if (Zoom > 3)
            {
                Zoom--;
                OffsetX = (OffsetX - Width / 2) / 2;
                OffsetY = (OffsetY - Height / 2) / 2;
            }
        }

        public void Resize(int w, int h)
        {
            OffsetX += (Width - w) / 2;
            OffsetY += (Height - h) / 2;

            Width = w;
            Height = h;
        }

        public void UpdateBounds()
        {
            int maxY = (1 << Zoom) * Tile.TileSize - Height;
            if (OffsetY < 0)
                OffsetY = 0;
            else if (OffsetY > maxY)
                OffsetY = maxY;

            OffsetX = Mod(OffsetX, Tile.TileSize * (1 << Zoom));
            Tiles.SetBounds(
                OffsetX / Tile.TileSize,
                OffsetY / Tile.TileSize,
                (OffsetX + Width) / Tile.TileSize,
                (OffsetY + Height) / Tile.TileSize,
                Zoom);
        }

        private int Mod(int x, int div)
        {
            x %= div;
            if (x < 0)
                x += div;
            return x;
        }
    }
}
