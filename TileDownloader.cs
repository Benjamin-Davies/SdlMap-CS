using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace SdlMapCS
{
    public class TileDownloader
    {
        private ConcurrentQueue<(Tile tile, IntPtr memory, int size)> Done
            = new ConcurrentQueue<(Tile tile, IntPtr memory, int size)>();
        private HttpClient HttpClient = new HttpClient();

        public int Active { get; private set; }

        public void Queue(Tile tile)
        {
            Active++;
            var task = Load(tile);
            task.Start();
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
            return Active;
        }
    }
}