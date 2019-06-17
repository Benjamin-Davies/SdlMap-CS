using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using static SDL2.SDL;
using static SDL2.SDL_image;

namespace SdlMapCS
{
    public class WebLoader : TileLoader
    {
        private Queue<Tile> Q = new Queue<Tile>();
        private ConcurrentQueue<(Tile tile, IntPtr surface)> Done
            = new ConcurrentQueue<(Tile, IntPtr)>();
        private HttpClient HttpClient = new HttpClient();
        private readonly Range Range;

        public WebLoader(Range range)
        {
            Range = range;
            HttpClient.DefaultRequestHeaders.TryAddWithoutValidation(
                "User-Agent", "sdlmap/1.0");
        }

        public int Active { get; private set; }

        public void Queue(Tile tile)
        {
            Q.Enqueue(tile);
        }

        private async Task Load(Tile tile)
        {
            var bytes = await HttpClient.GetByteArrayAsync(tile.Url);

            var memory = Marshal.AllocHGlobal(bytes.Length);
            Marshal.Copy(bytes, 0, memory, bytes.Length);
            var img = IMG_Load_RW(SDL_RWFromMem(memory, bytes.Length), 1);
            Marshal.FreeHGlobal(memory);

            Done.Enqueue((tile, img));
        }

        public int Work()
        {
            while (Done.TryDequeue(out var item))
            {
                item.tile.Load(item.surface);
                Active--;
            }
            if (Active <= 0)
            {
                if (Q.Count > 0)
                {
                    var tile = Q.Dequeue();
                    while (!Range.Contains(tile.X, tile.Y, tile.Zoom) && Q.Count > 0)
                        tile = Q.Dequeue();

                    tile.Queue();
                    Task.Run(() => Load(tile));
                    Active++;
                }
            }
            return Active;
        }
    }
}
