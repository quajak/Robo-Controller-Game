using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Engine
{
    public class GameWorld
    {
        public GameObject[,] map;

        public List<Entity> Entities { get; private set; }

        private readonly int width;
        private readonly int height;
        public const int fieldSize = 20;

        private readonly Random rng;

        public GameWorld(int Width, int Height, Point start, int seed = 0)
        {
            //TODO: Make better world generation
            rng = new Random(0);
            width = Width;
            height = Height;

            map = new GameObject[width, height];

            int ID = 0;
            for (int x = 0; x <= map.GetUpperBound(0); x++)
            {
                for (int y = 0; y <= map.GetUpperBound(1); y++)
                {
                    GameObject tile;
                    switch (rng.Next(10))
                    {
                        case int val when (val < 3):
                            tile = new Wall(ID++, new Point(x * fieldSize, y * fieldSize));
                            break;

                        case int val when (val < 6):
                            tile = new Boulder(ID++, new Point(x * fieldSize, y * fieldSize));
                            break;

                        default:
                            tile = new Space(ID++, new Point(x * fieldSize, y * fieldSize));
                            break;
                    }
                    map[x, y] = tile;
                }

                map[(int)start.X, (int)start.Y] = new GameObject(ID++, Colors.Transparent, new Point(start.X * fieldSize, start.Y * fieldSize));
            }
        }

        public void AddEntity(Entity entity)
        {
            Entities.Add(entity);
        }
    }
}