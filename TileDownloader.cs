using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace SdlMapCS
{
    public class TileDownloader
    {
        private Queue<Tile> Q = new Queue<Tile>();
        private ConcurrentQueue<(Tile tile, IntPtr memory, int size)> Done
            = new ConcurrentQueue<(Tile tile, IntPtr memory, int size)>();
        private HttpClient HttpClient = new HttpClient();

        public TileDownloader()
        {
            HttpClient.DefaultRequestHeaders.TryAddWithoutValidation(
                "User-Agent", "sdlmap/1.0");
        }

        public int Active { get; private set; }

        public void Queue(Tile tile)
        {
            Q.Enqueue(tile);
            tile.Queue();
        }

        private async Task Load(Tile tile)
        {
            var bytes = await HttpClient.GetByteArrayAsync(tile.Url);

            var memory = Marshal.AllocHGlobal(bytes.Length);
            Marshal.Copy(bytes, 0, memory, bytes.Length);

            Done.Enqueue((tile, memory, bytes.Length));
        }

        public int Work()
        {
            while (Done.TryDequeue(out var item))
            {
                item.tile.Load(item.memory, item.size);
                Marshal.FreeHGlobal(item.memory);
                Active--;
            }
            if (Active <= 0)
            {
                if (Q.Count > 0)
                {
                    var tile = Q.Dequeue();
                    Task.Run(() => Load(tile));
                    Active++;
                }
            }
            return Active;
        }
    }
}