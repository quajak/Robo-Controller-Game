using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;

namespace Engine
{
    public class Renderer
    {
        private Canvas canvas;

        private List<GameObject> gameObjects = new List<GameObject>();
        private List<Shape> shapes = new List<Shape>();
        private List<Image> images = new List<Image>();

        public Renderer(Canvas Canvas, List<GameObject> GameObjects)
        {
            gameObjects = GameObjects;
            gameObjects.ForEach(g => shapes.Add(
                new Rectangle()
                {
                    Name = "f" + g.id.ToString(),
                    Width = GameWorld.fieldSize,
                    Height = GameWorld.fieldSize,
                    Fill = new SolidColorBrush(g.color),
                    Stroke = new SolidColorBrush(g.color)
                }));

            canvas = Canvas;
            shapes.ForEach(s =>
            {
                canvas.Children.Add(s);
                int id = int.Parse(s.Name.Remove(0, 1));
                SetPosition(id);
            });
        }

        protected static ImageSource BitmapToImageSource(System.Drawing.Bitmap image)
        {
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(image.GetHbitmap(),
                IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }

        public void Update()
        {
            gameObjects.ForEach(g =>
           {
               if (g.updated)
               {
                   SetAbsolutePosition(g.id, g.position);
                   g.updated = false;
               }
           });
        }

        public void AddEntity(GameObject g, bool isImage = false)
        {
            gameObjects.Add(g);
            if (isImage)
            {
                Image img = new Image
                {
                    Name = "f" + g.id.ToString(),
                    Width = GameWorld.fieldSize,
                    Height = GameWorld.fieldSize,
                    Source = BitmapToImageSource(new System.Drawing.Bitmap(((ImageEntity)g).CurrentImage()))
                };
                canvas.Children.Add(img);
                SetPosition(g.id);
                images.Add(img);
                return;
            }
            Rectangle s = new Rectangle()
            {
                Name = "f" + g.id.ToString(),
                Width = GameWorld.fieldSize,
                Height = GameWorld.fieldSize,
                Fill = new SolidColorBrush(g.color),
                Stroke = new SolidColorBrush(g.color)
            };
            canvas.Children.Add(s);
            SetPosition(int.Parse(s.Name.Remove(0, 1)));
            shapes.Add(s);
        }

        public void RemoveEntity(GameObject g)
        {
            List<Shape> _Shape = new List<Shape>(shapes);
            foreach (Shape s in shapes)
            {
                if (s.Name == "f" + g.id.ToString())
                {
                    _Shape.Remove(s);
                    canvas.Children.Remove(s);
                }
            };
            shapes = _Shape;
            images.ForEach(i =>
            {
                if (i.Name == "f" + g.id.ToString())
                {
                    images.Remove(i);
                    canvas.Children.Remove(i);
                }
            });

            gameObjects.Remove(g);
        }

        public bool LoadScene()
        {
            return true;
        }

        private void SetPosition(GameObject g, Shape s)
        {
            Canvas.SetTop(s, g.position.Y * GameWorld.fieldSize);
            Canvas.SetLeft(s, g.position.X * GameWorld.fieldSize);
        }

        private void SetPosition(GameObject g, Image s)
        {
            Canvas.SetTop(s, g.position.Y * GameWorld.fieldSize);
            Canvas.SetLeft(s, g.position.X * GameWorld.fieldSize);
        }

        private void SetPosition(int Id, Point offset)
        {
            Vector vector = new Vector(offset.X, offset.Y);
            gameObjects.ForEach(g =>
            {
                if (Id == g.id)
                {
                    g.position += vector;
                    shapes.ForEach(r =>
                    {
                        if (r.Name == "f" + g.id.ToString())
                        {
                            Canvas.SetTop(r, g.position.Y);
                            Canvas.SetLeft(r, g.position.X);
                            return;
                        }
                    });
                    images.ForEach(i =>
                   {
                       if (i.Name == "f" + g.id.ToString())
                       {
                           SetPosition(g, i);
                           return;
                       }
                   });
                    return;
                }
            });
        }

        private void SetAbsolutePosition(int Id, Point position)
        {
            gameObjects.ForEach(g =>
            {
                if (Id == g.id)
                {
                    g.position = position;
                    shapes.ForEach(r =>
                    {
                        if (r.Name == "f" + g.id.ToString())
                        {
                            Canvas.SetTop(r, g.position.Y);
                            Canvas.SetLeft(r, g.position.X);
                            return;
                        }
                    });
                    images.ForEach(i =>
                   {
                       if (i.Name == "f" + g.id.ToString())
                       {
                           SetPosition(g, i);
                           return;
                       }
                   });
                    return;
                }
            });
        }

        private void SetPosition(int Id, int xOffset = 0, int yOffset = 0)
        {
            SetPosition(Id, new Point(xOffset, yOffset));
        }
    }
}