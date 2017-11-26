using Engine.Engine.WorldGeneration;
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
        public MapObject[,] map;

        private Room[,] roomMap;

        public List<Entity> Entities { get; private set; }

        private readonly int width;
        private readonly int height;
        public const int fieldSize = 20;

        private readonly Random rng;

        public GameWorld(int Width, int Height, Point start, int seed = 0)
        {
            ////TODO: Make better world generation
            rng = new Random(0);
            width = Width;
            height = Height;

            map = new MapObject[width, height];
            int ID = 0;
            //Fill map with air
            for (int x = 0; x <= map.GetUpperBound(0); x++)
            {
                for (int y = 0; y <= map.GetUpperBound(1); y++)
                {
                    map[x, y] = new Space(ID++, new Point(x, y));
                }
            }

            if (Width % Room.RoomBaseSize != 0 || Height % Room.RoomBaseSize != 0) throw new Exception("The large map can not be mapped onto the smaller!");
            roomMap = new Room[Width / Room.RoomBaseSize, Height / Room.RoomBaseSize];

            //Define all possible rooms
            Point _Position = new Point(0, 0);
            List<Room> possibilites = new List<Room>
            {
                new IntersectionRoom(_Position),
                new TIntersectionBottom(_Position),
                new TIntersectionTop(_Position),
                new TIntersectionLeft(_Position),
                new TIntersectionRight(_Position),
                new SingleEntranceRoomTop(_Position),
                new SingleEntranceRoomRight(_Position),
                new SingleEntranceRoomLeft(_Position),
                new SingleEntranceRoomBottom(_Position),
                new FilledRoom(_Position)
            };

            //Add start room
            roomMap[0, 0] = new StartRoom(new Point(0, 0));
            roomMap[0, 0].Generate(ref map, ref ID);

            for (int x = 0; x <= roomMap.GetUpperBound(0); x++)
            {
                for (int y = 0; y <= roomMap.GetUpperBound(1); y++)
                {
                    if (roomMap[x, y] != null) continue;

                    //Find possible rooms
                    //Find needed exits
                    List<int> top;
                    List<int> bottom;
                    List<int> left;
                    List<int> right;

                    if (y == 0) top = new List<int>();
                    else top = roomMap[x, y - 1].exits.bottom;

                    if (y == roomMap.GetUpperBound(1)) bottom = new List<int>();
                    else if (roomMap[x, y + 1] != null) bottom = roomMap[x, y + 1].exits.top;
                    else bottom = null;

                    if (x == 0) left = new List<int>();
                    else left = roomMap[x - 1, y].exits.right;

                    if (x == roomMap.GetUpperBound(0)) right = new List<int>();
                    else if (roomMap[x + 1, y] != null) right = roomMap[x + 1, y].exits.left;
                    else right = null;

                    Adjacent<List<int>> neededExits = new Adjacent<List<int>>(top, left, right, bottom);

                    //Find possible rooms
                    Room room = possibilites.First(p =>
                    {
                        bool _top = p.exits.top.SequenceEqual(neededExits.top);
                        bool _left = p.exits.left.SequenceEqual(neededExits.left);
                        bool _right = null == neededExits.right || p.exits.right.SequenceEqual(neededExits.right);
                        bool _bottom = null == neededExits.bottom || p.exits.bottom.SequenceEqual(neededExits.bottom);
                        return _top && _left && _right && _bottom;
                    });

                    //Add room
                    roomMap[x, y] = room.NewRoom(new Point(x * Room.RoomBaseSize, y * Room.RoomBaseSize));
                    roomMap[x, y].Generate(ref map, ref ID);
                }
            }
        }

        public void AddEntity(Entity entity)
        {
            Entities.Add(entity);
        }
    }

    namespace Engine.WorldGeneration
    {
        #region BaseDefinitions

        public abstract class Room
        {
            public const int RoomBaseSize = 5;

            protected Point position;
            private int width;
            private int height;
            private RoomPartGenerator[] roomPartGenerators;
            public Adjacent<List<int>> exits;

            internal Room(Point Position, int Width, int Height, RoomPartGenerator[] RoomPartGenerator, Adjacent<List<int>> Exits)
            {
                position = Position;
                width = Width;
                height = Height;
                roomPartGenerators = RoomPartGenerator;
                exits = Exits;
            }

            public void Generate(ref MapObject[,] map, ref int IDCounter)
            {
                foreach (RoomPartGenerator generator in roomPartGenerators) generator.Generate(map, position, ref IDCounter);
            }

            public abstract Room NewRoom(Point point);
        }

        internal abstract class RoomPartGenerator
        {
            public abstract void Generate(MapObject[,] map, Point Offset, ref int IDCounter);

            public void Generate(MapObject[,] map, int xOffset, int yOffset, ref int IDCounter)
            {
                Generate(map, new Point(xOffset, yOffset), ref IDCounter);
            }
        }

        #endregion BaseDefinitions

        #region RoomPartGenerator Definitions

        internal class Line : RoomPartGenerator
        {
            private Point start;
            private Point end;
            private RoomType type;

            public Line(Point Start, Point End, RoomType Type)
            {
                //TODO: Allow lines to change in both x and y values
                if (Start.X != End.X && Start.Y != End.Y) throw new NotImplementedException();
                if (Start.X > End.X || Start.Y > End.Y) throw new Exception("The dimensions of the line are wrong!");
                start = Start;
                end = End;
                type = Type;
            }

            public Line(int startX, int startY, int endX, int endY, RoomType Type) :
                this(new Point(startX, startY), new Point(endX, endY), Type)
            {
            }

            public override void Generate(MapObject[,] map, Point Offset, ref int IDCounter)
            {
                if (start.X == end.X)
                {
                    //Change in y value
                    for (int i = 0; i <= end.Y - start.Y; i++)
                    {
                        map[(int)start.X + (int)Offset.X, (int)start.Y + i + (int)Offset.Y] = MapObject.CreateRoomType(IDCounter++,
                            Utility.AddPoint(Offset, start) + new Vector(0, i), type);
                    }
                }
                else
                {
                    //Change in x value
                    for (int i = 0; i <= end.X - start.X; i++)
                    {
                        map[(int)start.X + i + (int)Offset.X, (int)start.Y + (int)Offset.Y] = MapObject.CreateRoomType(IDCounter++,
                            Utility.AddPoint(Offset, start) + new Vector(i, 0), type);
                    }
                }
            }
        }

        internal class Block : RoomPartGenerator
        {
            private Point position;
            private RoomType type;

            public Block(int X, int Y, RoomType Type) : this(new Point(X, Y), Type)
            {
            }

            public Block(Point Position, RoomType Type)
            {
                position = Position;
                type = Type;
            }

            public override void Generate(MapObject[,] map, Point Offset, ref int IDCounter)
            {
                map[(int)position.X + (int)Offset.X, (int)position.Y + (int)Offset.Y] = MapObject.CreateRoomType(IDCounter++, position, type);
            }
        }

        #endregion RoomPartGenerator Definitions

        #region Room Definitions

        internal class StartRoom : Room
        {
            public StartRoom(Point Position) : base(Position, 1, 1, new RoomPartGenerator[]{
                new Line(0, 0, 4, 0, RoomType.Wall),
                new Line(0, 0, 0, 4, RoomType.Wall),
                new Line(0, 4, 4, 4, RoomType.Wall),
                new Line(4, 0, 4, 4, RoomType.Wall),
                new Block(2, 4, RoomType.Air),
                new Block(4, 2, RoomType.Air)
            }, new Adjacent<List<int>>(new List<int>(), new List<int>(),
                new List<int>() { 2 }, new List<int> { 2 })
                )
            {
                position = Position;
            }

            public override Room NewRoom(Point position)
            {
                return new StartRoom(position);
            }
        }

        internal class IntersectionRoom : Room
        {
            public IntersectionRoom(Point position) : base(position, 1, 1, new RoomPartGenerator[]
            {
                new Line(0, 0, 4, 0, RoomType.Wall),
                new Line(0, 0, 0, 4, RoomType.Wall),
                new Line(0, 4, 4, 4, RoomType.Wall),
                new Line(4, 0, 4, 4, RoomType.Wall),
                new Block(2, 0, RoomType.Air),
                new Block(0, 2, RoomType.Air),
                new Block(2, 4, RoomType.Air),
                new Block(4, 2, RoomType.Air)
            }, new Adjacent<List<int>>(new List<int> { 2 }, new List<int> { 2 }, new List<int> { 2 },
                new List<int> { 2 }))
            {
            }

            public override Room NewRoom(Point point)
            {
                return new IntersectionRoom(point);
            }
        }

        internal class TIntersectionTop : Room
        {
            public TIntersectionTop(Point point) : base(point, 1, 1, new RoomPartGenerator[]
            {
                new Line(0, 0, 4, 0, RoomType.Wall),
                new Line(0, 0, 0, 4, RoomType.Wall),
                new Line(0, 4, 4, 4, RoomType.Wall),
                new Line(4, 0, 4, 4, RoomType.Wall),
                new Block(0, 2, RoomType.Air),
                new Block(2, 4, RoomType.Air),
                new Block(4, 2, RoomType.Air)
            }, new Adjacent<List<int>>(new List<int>(), new List<int>() { 2 },
                new List<int>() { 2 }, new List<int>() { 2 }))
            {
            }

            public override Room NewRoom(Point point)
            {
                return new TIntersectionTop(point);
            }
        }

        internal class TIntersectionLeft : Room
        {
            public TIntersectionLeft(Point point) : base(point, 1, 1, new RoomPartGenerator[]
            {
                new Line(0, 0, 4, 0, RoomType.Wall),
                new Line(0, 0, 0, 4, RoomType.Wall),
                new Line(0, 4, 4, 4, RoomType.Wall),
                new Line(4, 0, 4, 4, RoomType.Wall),
                new Block(2, 0, RoomType.Air),
                new Block(2, 4, RoomType.Air),
                new Block(4, 2, RoomType.Air)
            }, new Adjacent<List<int>>(new List<int>() { 2 }, new List<int>(),
                new List<int>() { 2 }, new List<int>() { 2 }))
            {
            }

            public override Room NewRoom(Point point)
            {
                return new TIntersectionLeft(point);
            }
        }

        internal class TIntersectionRight : Room
        {
            public TIntersectionRight(Point point) : base(point, 1, 1, new RoomPartGenerator[]
            {
                new Line(0, 0, 4, 0, RoomType.Wall),
                new Line(0, 0, 0, 4, RoomType.Wall),
                new Line(0, 4, 4, 4, RoomType.Wall),
                new Line(4, 0, 4, 4, RoomType.Wall),
                new Block(0, 2, RoomType.Air),
                new Block(2, 4, RoomType.Air),
                new Block(2, 0, RoomType.Air)
            }, new Adjacent<List<int>>(new List<int>() { 2 }, new List<int>() { 2 },
                new List<int>(), new List<int>() { 2 }))
            {
            }

            public override Room NewRoom(Point point)
            {
                return new TIntersectionRight(point);
            }
        }

        internal class TIntersectionBottom : Room
        {
            public TIntersectionBottom(Point point) : base(point, 1, 1, new RoomPartGenerator[]
            {
                new Line(0, 0, 4, 0, RoomType.Wall),
                new Line(0, 0, 0, 4, RoomType.Wall),
                new Line(0, 4, 4, 4, RoomType.Wall),
                new Line(4, 0, 4, 4, RoomType.Wall),
                new Block(0, 2, RoomType.Air),
                new Block(2, 0, RoomType.Air),
                new Block(4, 2, RoomType.Air)
            }, new Adjacent<List<int>>(new List<int>() { 2 }, new List<int>() { 2 },
                new List<int>() { 2 }, new List<int>()))
            {
            }

            public override Room NewRoom(Point point)
            {
                return new TIntersectionBottom(point);
            }
        }

        internal class SingleEntranceRoomTop : Room
        {
            public SingleEntranceRoomTop(Point point) : base(point, 1, 1, new RoomPartGenerator[]
            {
                new Line(0,0, 4, 0, RoomType.Wall),
                new Line(0, 0, 0, 4, RoomType.Wall),
                new Line(4, 0, 4, 4, RoomType.Wall),
                new Line(0, 4, 4, 4, RoomType.Wall),
                new Block(2, 0, RoomType.Air)
            }, new Adjacent<List<int>>(new List<int> { 2 }, new List<int>(), new List<int>(), new List<int>()))
            {
            }

            public override Room NewRoom(Point point)
            {
                return new SingleEntranceRoomTop(point);
            }
        }

        internal class SingleEntranceRoomLeft : Room
        {
            public SingleEntranceRoomLeft(Point point) : base(point, 1, 1, new RoomPartGenerator[]
            {
                new Line(0,0, 4, 0, RoomType.Wall),
                new Line(0, 0, 0, 4, RoomType.Wall),
                new Line(4, 0, 4, 4, RoomType.Wall),
                new Line(0, 4, 4, 4, RoomType.Wall),
                new Block(0, 2, RoomType.Air)
            }, new Adjacent<List<int>>(new List<int> { }, new List<int>() { 2 }, new List<int>(), new List<int>()))
            {
            }

            public override Room NewRoom(Point point)
            {
                return new SingleEntranceRoomLeft(point);
            }
        }

        internal class SingleEntranceRoomBottom : Room
        {
            public SingleEntranceRoomBottom(Point point) : base(point, 1, 1, new RoomPartGenerator[]
            {
                new Line(0,0, 4, 0, RoomType.Wall),
                new Line(0, 0, 0, 4, RoomType.Wall),
                new Line(4, 0, 4, 4, RoomType.Wall),
                new Line(0, 4, 4, 4, RoomType.Wall),
                new Block(2, 4, RoomType.Air)
            }, new Adjacent<List<int>>(new List<int> { }, new List<int>(), new List<int>() { },
                new List<int>() { 2 }))
            {
            }

            public override Room NewRoom(Point point)
            {
                return new SingleEntranceRoomBottom(point);
            }
        }

        internal class SingleEntranceRoomRight : Room
        {
            public SingleEntranceRoomRight(Point point) : base(point, 1, 1, new RoomPartGenerator[]
            {
                new Line(0,0, 4, 0, RoomType.Wall),
                new Line(0, 0, 0, 4, RoomType.Wall),
                new Line(4, 0, 4, 4, RoomType.Wall),
                new Line(0, 4, 4, 4, RoomType.Wall),
                new Block(4, 2, RoomType.Air)
            }, new Adjacent<List<int>>(new List<int> { }, new List<int>(), new List<int>(), new List<int>() { 2 }))
            {
            }

            public override Room NewRoom(Point point)
            {
                return new SingleEntranceRoomRight(point);
            }
        }

        internal class FilledRoom : Room
        {
            public FilledRoom(Point point) : base(point, 1, 1, new RoomPartGenerator[]
            {
                new Line(0, 0, 4, 0, RoomType.Wall),
                new Line(0, 0, 0, 4, RoomType.Wall),
                new Line(4, 0, 4, 4, RoomType.Wall),
                new Line(0, 4, 4, 4, RoomType.Wall)
            }, new Adjacent<List<int>>(new List<int>(), new List<int>(), new List<int>(), new List<int>()))
            {
            }

            public override Room NewRoom(Point point)
            {
                return new FilledRoom(point);
            }
        }

        #endregion Room Definitions
    }
}