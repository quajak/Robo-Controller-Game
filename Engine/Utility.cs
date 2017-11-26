using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Engine
{
    internal static class Utility
    {
        public static Point AddPoint(Point A, Point B)
        {
            return A + new Vector(B.X, B.Y);
        }
    }

    public struct Adjacent<T>
    {
        public T top;
        public T left;
        public T right;
        public T bottom;
        public T[] raw;

        public Adjacent(T Top, T Left, T Right, T Bottom)
        {
            top = Top;
            left = Left;
            right = Right;
            bottom = Bottom;
            raw = new T[] { Top, Left, Right, Bottom };
        }
    }
}