using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Engine
{
    public class GameObject
    {
        public Point position;
        public Color color;
        public readonly int id;
        private int angle;
        public bool updated = false;

        public bool blocking;
        public List<Entity> entities;
        public bool destroyable;
        public int health;

        public int Angle
        {
            get { return angle; }
            set
            {
                angle = value;
                if (angle < 0) angle += 360;
                if (angle >= 360) angle -= 360;
            }
        }

        public GameObject(int Id, Color Color, Point Position, bool Blocking = false)
        {
            blocking = Blocking;
            angle = 0;
            id = Id;
            color = Color;
            position = Position;
            entities = new List<Entity>();
        }
    }

    public class Space : GameObject
    {
        public Space(int ID, Point Position) : base(ID, Colors.Transparent, Position)
        {
        }
    }

    public class Wall : GameObject
    {
        public Wall(int ID, Point Position) : base(ID, Colors.Black, Position, true)
        {
        }
    }

    public class Boulder : GameObject
    {
        public Boulder(int ID, Point Position) : base(ID, Colors.DarkGray, Position, true)
        {
        }
    }

    public class Entity : GameObject
    {
        public Predicate<Entity> EnterField;
        public GameObject link;

        public Entity(int ID, Color Color, Point Position) : base(ID, Color, Position)
        {
        }
    }

    public class Robot : Entity
    {
        public int CPUSpeed = 1000; //Milliseconds per command

        public Robot(int id, Point start, GameWorld gameWorld) : base(id, Colors.Red, start)
        {
            EnterField = e =>
            {
                GameObject field;
                Vector movement;
                switch (e.Angle)
                {
                    case (0) when (e.position.X != gameWorld.map.GetUpperBound(0)):
                        field = gameWorld.map[(int)e.position.X + 1, (int)e.position.Y];
                        movement = new Vector(1, 0);
                        break;

                    case (90) when (e.position.Y != 0):
                        field = gameWorld.map[(int)e.position.X, (int)e.position.Y - 1];
                        movement = new Vector(0, -1);
                        break;

                    case (180) when (e.position.X != 0):
                        field = gameWorld.map[(int)e.position.X - 1, (int)e.position.Y];
                        movement = new Vector(-1, 0);
                        break;

                    case (270) when (e.position.Y != gameWorld.map.GetUpperBound(1)):
                        field = gameWorld.map[(int)e.position.X, (int)e.position.Y + 1];
                        movement = new Vector(0, 1);
                        break;

                    case int value:
                        return false;

                    default:
                        throw new Exception("The angle has a wrong value!");
                }
                if (!field.blocking && field.entities.TrueForAll(E => !E.blocking))
                {
                    //Change map tile owner
                    e.link.entities.Remove(e);
                    field.entities.Add(e);
                    e.link = field;
                    //Now move robot on field

                    e.position += movement;

                    e.updated = true;
                    return true;
                }
                else
                {
                    //TODO: Calcuate damage + collision
                    return false;
                }
            };
            link = gameWorld.map[0, 0];
        }
    }
}