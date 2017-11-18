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
using System.Windows.Media.Animation;

namespace Engine
{
    public class Renderer
    {
        private Canvas canvas;

        private List<GameObject> gameObjects = new List<GameObject>();
        private List<Shape> shapes = new List<Shape>();
        private List<Image> images = new List<Image>();

        private GameController gameController;

        public Renderer(GameController game, Canvas Canvas, List<GameObject> GameObjects)
        {
            gameController = game;
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

        private int runningAnimations = 0;

        private void StartAnimation()
        {
            if (runningAnimations == 0) gameController.inAnimation = true;
            runningAnimations++;
        }

        private void EndAnimation(object sender, EventArgs e)
        {
            runningAnimations--;
            if (runningAnimations == 0) gameController.inAnimation = false;
        }

        private void AnimatePositionChange(GameObject g)

        {
            List<Shape> list = shapes.Where(S => S.Name == "f" + g.id.ToString()).ToList();
            if (list.Count != 0)
            {
                StartAnimation();
                StartAnimation();
                Shape img = shapes[0];
                Vector offset = VisualTreeHelper.GetOffset(img);
                var left = offset.X + img.RenderTransform.Value.OffsetX;
                var top = offset.Y + img.RenderTransform.Value.OffsetY;
                TranslateTransform trans = new TranslateTransform();
                img.RenderTransform = trans;
                DoubleAnimation anim1 = new DoubleAnimation(top, g.position.Y * GameWorld.fieldSize, TimeSpan.FromSeconds(1));
                DoubleAnimation anim2 = new DoubleAnimation(left, g.position.X * GameWorld.fieldSize, TimeSpan.FromSeconds(1));
                anim1.Completed += EndAnimation;
                anim2.Completed += EndAnimation;
                trans.BeginAnimation(TranslateTransform.YProperty, anim1);
                trans.BeginAnimation(TranslateTransform.XProperty, anim2);
                return;
            }
            List<Image> image = images.Where(i => i.Name == "f" + g.id.ToString()).ToList();
            if (image.Count != 0)
            {
                StartAnimation();
                StartAnimation();
                Image img = image[0];
                Vector offset = VisualTreeHelper.GetOffset(img);
                var left = offset.X + img.RenderTransform.Value.OffsetX;
                var top = offset.Y + img.RenderTransform.Value.OffsetY;
                TranslateTransform trans = ((TransformGroup)img.RenderTransform).Children[1] as TranslateTransform;
                DoubleAnimation anim1 = new DoubleAnimation(top, g.position.Y * GameWorld.fieldSize, TimeSpan.FromSeconds(1));
                DoubleAnimation anim2 = new DoubleAnimation(left, g.position.X * GameWorld.fieldSize, TimeSpan.FromSeconds(1));
                anim1.Completed += EndAnimation;
                anim2.Completed += EndAnimation;
                trans.BeginAnimation(TranslateTransform.YProperty, anim1);
                trans.BeginAnimation(TranslateTransform.XProperty, anim2);
                return;
            }
        }

        public void Update()
        {
            gameObjects.ForEach(g =>
           {
               if (g.updated)
               {
                   if (g.animate)
                   {
                       if (g.animationType == AnimationType.movement) AnimatePositionChange(g);
                       if (g.animationType == AnimationType.rotation) AnimateRotationChange(g);
                   }
                   else
                   {
                       SetAbsolutePosition(g.id, g.position);
                   }
                   g.animate = false;
                   g.updated = false;
               }
           });
        }

        private void AnimateRotationChange(GameObject g)
        {
            List<Shape> list = shapes.Where(S => S.Name == "f" + g.id.ToString()).ToList();
            if (list.Count != 0)
            {
                throw new System.NotImplementedException();
            }
            List<Image> image = images.Where(i => i.Name == "f" + g.id.ToString()).ToList();
            if (image.Count != 0)
            {
                StartAnimation();
                Image img = image[0];
                //Angles are to left = negative Top is 0
                //000 090 180 270 -> Gameobject
                //090 000 270 180 -> Control
                //
                //000 001 002 003
                //090 -90 090 -90
                //Therefor if even +90 else -90
                //TODO: ACTUAL SOLVE THE PROBLEM

                int angle = 0;
                if ((g.Angle / 90) % 2 == 0) angle = 90;
                else angle = -90;

                DoubleAnimation anim1 = new DoubleAnimation(g.Angle + angle, TimeSpan.FromSeconds(1));
                anim1.Completed += EndAnimation;
                ((RotateTransform)((TransformGroup)img.RenderTransform).Children[0]).
                    BeginAnimation(RotateTransform.AngleProperty, anim1);
                return;
            }
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
                    Source = BitmapToImageSource(new System.Drawing.Bitmap(((ImageEntity)g).CurrentImage())),
                    RenderTransform = new TransformGroup(),
                    RenderTransformOrigin = new Point(0.5, 0.5)
                };
                TransformGroup transformGroup = (TransformGroup)img.RenderTransform;
                transformGroup.Children.Add(new RotateTransform());
                ((RotateTransform)transformGroup.Children[0]).Angle = 90; //Rotation is different way around
                transformGroup.Children.Add(new TranslateTransform());
                img.RenderTransform = transformGroup;

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