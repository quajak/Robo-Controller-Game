﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Engine
{
    public enum AnimationType { movement, rotation };

    public class GameObject
    {
        public Point position;
        public Color color;
        public readonly int id;
        private int angle = 0;
        private int oldAngle;

        public bool updated = false;
        public AnimationType animationType;
        public bool animate = false;

        public bool blocking;
        public List<Entity> entities;

        public int health;

        public Dimension size;

        public int Angle
        {
            get { return angle; }
            set
            {
                oldAngle = angle;
                angle = value;
                if (angle < 0) angle += 360;
                if (angle >= 360) angle -= 360;
            }
        }

        public int OldAngle
        {
            get { return oldAngle; }
        }

        public GameObject(int Id, Color Color, Point Position, bool Blocking = false, Dimension Size = null)
        {
            if (Size is null) Size = new Dimension(1, 1);

            size = Size;
            blocking = Blocking;
            angle = 0;
            id = Id;
            color = Color;
            position = Position;
            entities = new List<Entity>();
        }
    }

    public enum RoomType { Air, Wall, Boulder };

    public class MapObject : GameObject
    {
        public bool mineable;

        public MapObject(int ID, Color Color, Point Position, bool blocking = false, bool Mineable = false) : base(ID, Color, Position, blocking)
        {
            mineable = Mineable;
        }

        public static MapObject CreateRoomType(int ID, Point position, RoomType type)
        {
            switch (type)
            {
                case RoomType.Air:

                    return new Space(ID, position);

                case RoomType.Wall:
                    return new Wall(ID, position);

                case RoomType.Boulder:
                    return new Boulder(ID, position);

                default:
                    throw new Exception("Unknown type!");
            }
        }
    }

    public class Space : MapObject
    {
        public Space(int ID, Point Position) : base(ID, Colors.Transparent, Position)
        {
        }
    }

    public class Wall : MapObject
    {
        public Wall(int ID, Point Position) : base(ID, Colors.Black, Position, true, false)
        {
        }
    }

    public class Boulder : MapObject
    {
        public Boulder(int ID, Point Position) : base(ID, Colors.DarkGray, Position, true, true)
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

    public abstract class ImageEntity : Entity
    {
        public bool updateImage = false;

        public ImageEntity(int ID, Color Color, Point Position) : base(ID, Color, Position)
        {
        }

        public abstract System.Drawing.Image CurrentImage();
    }

    public class Robot : ImageEntity
    {
        public int CPUSpeed = 1000; //Milliseconds per command
        public int MAXRAMSize;
        public List<RobotEquipment> equipment;

        public override System.Drawing.Image CurrentImage()
        {
            //Sort the equipment
            List<RobotEquipment> Equipment = equipment.Where(e => e.drawingLevel != -1).ToList();
            Equipment.Sort((a, b) => a.drawingLevel - b.drawingLevel);

            System.Drawing.Image image = new System.Drawing.Bitmap(64, 64);

            using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(image))
            {
                g.DrawRectangle(System.Drawing.Pens.Transparent, 0, 0, 64, 64);
                foreach (RobotEquipment part in Equipment)
                {
                    g.DrawImage(ImageWpfToGdi2(part.robotImage), 0, 0);
                }
            }

            return image;
        }

        public System.Drawing.Image ImageWpfToGdi2(ImageSource image)
        {
            var ms = new System.IO.MemoryStream();
            var encoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
            encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(
                    (System.Windo‌​ws.Media.Imaging.Bit‌​mapSource)image));
            encoder.Save(ms);
            ms.Flush();
            var btm = (System.Drawing.Bitmap)System.Drawing.Image.FromStream(ms);
            return btm;
        }

        public Robot(int id, Point start, GameController gameController, List<RobotEquipment> parts) : base(id, Colors.Red, start)
        {
            equipment = new List<RobotEquipment>(parts);

            EnterField = e =>
            {
                Point movement = Programm.GetOffset(e.Angle, e.position, gameController.gameWorld, out MapObject field);
                if (movement == new Point(0, 0)) return true;
                if (!field.blocking && field.entities.TrueForAll(E => !E.blocking))
                {
                    //Change map tile owner
                    e.link.entities.Remove(e);
                    field.entities.Add(e);
                    e.link = field;
                    //Now move robot on field

                    e.position += movement;
                    return true;
                }
                else
                {
                    //TODO: Calcuate damage + collision
                    return false;
                }
            };
            link = gameController.gameWorld.map[0, 0];
        }
    }
}