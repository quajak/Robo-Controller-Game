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

    public class Point
    {
        public int x;
        public int y;

        public Point(int X, int Y)
        {
            x = X;
            y = Y;
        }

        public static Point operator +(Point A, Point B)
        {
            Point C = new Point(A.x, A.y);
            C.x += B.x;
            C.y += B.y;
            return C;
        }

        public static Point operator -(Point A, Point B)
        {
            Point C = new Point(A.x, A.y);
            C.x -= B.x;
            C.y -= B.y;
            return C;
        }

        public static Point operator *(Point A, int B)
        {
            Point C = new Point(A.x, A.y);
            C.x *= B;
            C.y *= B;
            return C;
        }

        public static bool operator ==(Point A, Point B)
        {
            return A.x == B.x && A.y == B.y;
        }

        public static bool operator !=(Point A, Point B)
        {
            return !(A == B);
        }

        public System.Windows.Point NatrualPoint()
        {
            return new System.Windows.Point(x, y);
        }
    }

    public class Dimension
    {
        public int Width;
        public int Height;

        public Dimension(int width, int height)
        {
            Width = width;
            Height = height;
        }
    }
}