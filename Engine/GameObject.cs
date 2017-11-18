using System;
using System.Collections.Generic;
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
        private int angle;
        private int oldAngle;

        public bool updated = false;
        public AnimationType animationType;
        public bool animate = false;

        public bool blocking;
        public List<Entity> entities;

        public int health;

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

    public class MapObject : GameObject
    {
        public bool mineable;

        public MapObject(int ID, Color Color, Point Position, bool blocking = false, bool Mineable = false) : base(ID, Color, Position, blocking)
        {
            mineable = Mineable;
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
        public ImageEntity(int ID, Color Color, Point Position) : base(ID, Color, Position)
        {
        }

        public abstract System.Drawing.Image CurrentImage();
    }

    public class Robot : ImageEntity
    {
        public int CPUSpeed = 1000; //Milliseconds per command
        public List<RobotEquipment> equipment;

        public override System.Drawing.Image CurrentImage()
        {
            //Sort the equipment
            equipment = equipment.Where(e => e.drawingLevel != -1).ToList();
            equipment.Sort((a, b) => a.drawingLevel - b.drawingLevel);

            System.Drawing.Image image = new System.Drawing.Bitmap(64, 64);

            using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(image))
            {
                g.DrawRectangle(System.Drawing.Pens.Transparent, 0, 0, 64, 64);
                foreach (RobotEquipment part in equipment)
                {
                    g.DrawImage(ImageWpfToGdi2(part.image), 0, 0);
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
            equipment = parts;
            EnterField = e =>
            {
                Vector movement = LanguageExecuter.GetOffset(e.Angle, e.position, gameController.gameWorld, out MapObject field);
                if (movement == new Vector(0, 0)) return true;
                if (!field.blocking && field.entities.TrueForAll(E => !E.blocking))
                {
                    //Change map tile owner
                    e.link.entities.Remove(e);
                    field.entities.Add(e);
                    e.link = field;
                    //Now move robot on field

                    e.position += movement;

                    e.updated = true;
                    e.animationType = AnimationType.movement;
                    e.animate = true;
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