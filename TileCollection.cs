﻿using System;
using System.Collections.Generic;

namespace SdlMapCS
{
    public struct Range
    {
        public int MinX, MinY, MaxX, MaxY, Depth;

        public bool Contains(int x, int y, int d)
        {
            if (d > Depth)
                throw new ArgumentOutOfRangeException(nameof(d));

            int dz = Depth - d;
            return x >= MinX >> dz
                && y >= MinY >> dz
                && x <= MaxX >> dz
                && y <= MaxY >> dz;
        }
    }

    public class Quad
    {
        private Tile Tile;
        private readonly Quad[] Children = new Quad[4];
        private readonly int X, Y, Depth;

        public Quad(int x, int y, int depth)
        {
            X = x;
            Y = y;
            Depth = depth;
        }

        public void QueryRange(out List<Tile> l, Range r)
        {
            l = new List<Tile>();

            if (!r.Contains(X, Y, Depth))
                return;

            if (Tile == null)
                Tile = new Tile(X, Y, Depth);

            if (Depth < r.Depth)
            {
                BuildChildren();
                for (int i = 0; i < 4; i++)
                    Children[i].QueryRange(l, r);
            }
            else
            {
                l.Add(Tile);
            }
        }

        private void BuildChildren()
        {
            if (Children[0] == null)
                Children[0] = new Quad(X * 2, Y * 2, Depth + 1);
            if (Children[1] == null)
                Children[1] = new Quad(X * 2 + 1, Y * 2, Depth + 1);
            if (Children[2] == null)
                Children[2] = new Quad(X * 2, Y * 2 + 1, Depth + 1);
            if (Children[3] == null)
                Children[3] = new Quad(X * 2 + 1, Y * 2 + 1, Depth + 1);
        }
    }

    public class TileCollection
    {
        private Quad Quad;
        private Range Range;

        public TileDownloader Transfers;

        public TileCollection(int minX, int minY, int maxX, int maxY, int zoom)
        {
            Range = new Range
            {
                MinX = minX,
                MinY = minY,
                MaxX = maxX,
                MaxY = maxY,
                Depth = zoom
            };
        }

        public void SetBounds(int minX, int minY, int maxX, int maxY, int zoom)
        {
            Range.MinX = minX;
            Range.MinY = minY;
            Range.MaxX = maxX;
            Range.MaxY = maxY;
            Range.Depth = zoom;
        }

        public bool Work()
        {
            bool working = Transfers.Active();
            Transfers.Work();

            Quad.QueryRange(out var l, Range);

            foreach (var t in l)
            {
                if (t.State != TileState.Empty)
                    continue;
                Transfers.Queue(t);
                working = true;
            }
            return working;
        }

        void Render(IntPtr screen, int offsetX, int offsetY)
        {
            Quad.QueryRange(out var l, Range);
            foreach (var t in l)
            {
                t.Render(screen, offsetX, offsetY);
            }
        }
    }
}