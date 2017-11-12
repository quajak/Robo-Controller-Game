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

namespace CanvasRenderer
{
    public class Renderer
    {
        private Canvas canvas;

        private List<GameObject> gameObjects = new List<GameObject>();
        private List<Rectangle> shapes = new List<Rectangle>();

        public Renderer(Canvas Canvas, List<GameObject> GameObjects)
        {
            gameObjects = GameObjects;
            gameObjects.ForEach(g => shapes.Add(
                new Rectangle()
                {
                    Name = "f" + g.id.ToString(),
                    Width = 20,
                    Height = 20,
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

        public void AddEntity(GameObject g)
        {
            gameObjects.Add(g);
            Rectangle s = new Rectangle()
            {
                Name = "e" + g.id.ToString(),
                Width = 20,
                Height = 20,
                Fill = new SolidColorBrush(g.color),
                Stroke = new SolidColorBrush(g.color)
            };
            canvas.Children.Add(s);
            SetPosition(int.Parse(s.Name.Remove(0, 1)));
            shapes.Add(s);
        }

        public bool LoadScene()
        {
            return true;
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
                        }
                    });
                }
            });
        }

        private void SetPosition(int Id, int xOffset = 0, int yOffset = 0)
        {
            SetPosition(Id, new Point(xOffset, yOffset));
        }
    }
}