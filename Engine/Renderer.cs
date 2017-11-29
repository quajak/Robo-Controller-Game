using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Rendering
{
    public class Renderer
    {
        public int RunningAnimations = 0;

        internal static int FieldSize = GameWorld.fieldSize;
        private static Dimension FieldDimension = new Dimension(FieldSize, FieldSize);

        private System.Windows.Controls.Canvas canvas;

        private List<RenderObject> renderObjects = new List<RenderObject>();

        private GameController gameController;

        //This is assumed to be in fields not in pixels
        private Dimension map;

        private Dimension screen;
        internal Point offset;

        private Robot robot;

        public Renderer(System.Windows.Controls.Canvas Canvas, GameController controller, List<GameObject> gameObjects)
        {
            //TODO: ALlow canvas to not be centered around robot!
            //TODO: Allow game objects added this way to be images or any type other than just rectangle

            #region Parameter checking

            if (Canvas.Width % FieldSize != 0 || Canvas.Height % FieldSize != 0) throw new Exception("The canvas size is not a multiple of a game object size");

            #endregion Parameter checking

            #region Variable Declaration

            canvas = Canvas;
            gameController = controller;
            robot = gameController.robot;

            map = new Dimension(gameController.gameWorld.map.GetUpperBound(0), gameController.gameWorld.map.GetUpperBound(1));
            screen = new Dimension((int)canvas.Width / FieldSize, (int)canvas.Height / FieldSize);
            offset = robot.position;

            #endregion Variable Declaration

            #region Add GameObjects

            gameObjects.ForEach(g => renderObjects.Add(new RenderObject(g, null, RenderObjectType.rectangle, FieldDimension, canvas, this)));

            #endregion Add GameObjects

            Update();
        }

        internal static System.Windows.Media.ImageSource BitmapToImageSource(System.Drawing.Bitmap image)
        {
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(image.GetHbitmap(),
                IntPtr.Zero, System.Windows.Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
        }

        public void AddEntity(GameObject gameObject, RenderObjectType type)
        {
            RenderObject item = new RenderObject(gameObject, null, type, gameObject.size, canvas, this);
            renderObjects.Add(item);
            //Set the correct offset
            item.Update(forced: true);
        }

        public void RemoveEntity(GameObject gameObject)
        {
            renderObjects.Where(r => r.ID == gameObject.id).ToList().ForEach(g => g.Delete());
            renderObjects.RemoveAll(r => r.ID == gameObject.id);
        }

        public void Update()
        {
            //Robot should be on middle tile
            Point expectedOffset = new Point(0, 0);
            int robotWidth = robot.size.Width * FieldSize / 2;
            int robotHeight = robot.size.Height * FieldSize / 2;

            expectedOffset.x = screen.Width * FieldSize / 2 - robotWidth - robot.position.x * FieldSize;
            expectedOffset.y = screen.Height * FieldSize / 2 - robotHeight - robot.position.y * FieldSize;

            if (!robot.animate && offset != expectedOffset)
            {
                offset = expectedOffset;
                renderObjects.ForEach(r => { r.gameObject.animate = true; r.Update(forced: true); });
            }
            else renderObjects.ForEach(r => r.Update());
        }

        public void EndAnimation(object sender, EventArgs e)
        {
            RunningAnimations--;
            if (RunningAnimations == 0) Update();
        }
    }

    public enum RenderObjectType
    { rectangle, image };

    internal class RenderObject
    {
        private Renderer renderer;

        internal GameObject gameObject;
        private System.Windows.UIElement uIElement;
        private RenderObjectType type;
        private Dimension size;

        private System.Windows.Media.TranslateTransform translateTransform;
        private System.Windows.Media.RotateTransform rotateTransform;

        private System.Windows.Controls.Canvas canvas;

        public int ID { get { return gameObject.id; } }

        public RenderObject(GameObject GameObject, System.Windows.UIElement element, RenderObjectType objectType, Dimension Size, System.Windows.Controls.Canvas Canvas, Renderer controller)
        {
            //Create new UI Element
            if (element is null)
            {
                switch (objectType)
                {
                    case RenderObjectType.rectangle:
                        element = new System.Windows.Shapes.Rectangle()
                        {
                            Width = Size.Width,
                            Height = Size.Height,
                            Fill = new System.Windows.Media.SolidColorBrush(GameObject.color),
                            Stroke = new System.Windows.Media.SolidColorBrush(GameObject.color),
                            RenderTransform = new System.Windows.Media.TransformGroup(),
                            RenderTransformOrigin = new System.Windows.Point(0.5, 0.5)
                        };
                        System.Windows.Media.TransformGroup transformGroup = (System.Windows.Media.TransformGroup)element.RenderTransform;
                        rotateTransform = new System.Windows.Media.RotateTransform(90); //Rotation is different system than used
                        transformGroup.Children.Add(rotateTransform);
                        translateTransform = new System.Windows.Media.TranslateTransform(GameObject.position.x * Renderer.FieldSize, GameObject.position.y * Renderer.FieldSize);
                        transformGroup.Children.Add(translateTransform);
                        element.RenderTransform = transformGroup;
                        break;

                    case RenderObjectType.image:
                        element = new System.Windows.Controls.Image()
                        {
                            Width = Size.Width * Renderer.FieldSize, //TODO: Fix the difference in size between the two types
                            Height = Size.Height * Renderer.FieldSize,
                            Source = Renderer.BitmapToImageSource(new System.Drawing.Bitmap(((ImageEntity)GameObject).CurrentImage())),
                            RenderTransform = new System.Windows.Media.TransformGroup(),
                            RenderTransformOrigin = new System.Windows.Point(0.5, 0.5)
                        };
                        transformGroup = (System.Windows.Media.TransformGroup)element.RenderTransform;
                        rotateTransform = new System.Windows.Media.RotateTransform(90); //Rotation is different system than used
                        transformGroup.Children.Add(rotateTransform);
                        translateTransform = new System.Windows.Media.TranslateTransform(GameObject.position.x * Renderer.FieldSize, GameObject.position.y * Renderer.FieldSize);
                        transformGroup.Children.Add(translateTransform);
                        element.RenderTransform = transformGroup;
                        break;

                    default:
                        break;
                }
            }

            gameObject = GameObject;
            uIElement = element;
            type = objectType;
            size = Size;
            renderer = controller;
            canvas = Canvas;

            Canvas.Children.Add(uIElement);
        }

        public void Delete()
        {
            canvas.Children.Remove(uIElement);
        }

        private void UpdatePosition(bool animated = true)
        {
            if (animated)
            {
                renderer.RunningAnimations++;
                renderer.RunningAnimations++;

                System.Windows.Media.Animation.DoubleAnimation animX = new System.Windows.Media.Animation.DoubleAnimation(
                    gameObject.position.x * Renderer.FieldSize + renderer.offset.x, TimeSpan.FromSeconds(1));
                System.Windows.Media.Animation.DoubleAnimation animY = new System.Windows.Media.Animation.DoubleAnimation(
                    gameObject.position.y * Renderer.FieldSize + renderer.offset.y, TimeSpan.FromSeconds(1));
                animX.Completed += renderer.EndAnimation;
                animY.Completed += renderer.EndAnimation;
                translateTransform.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty, animX);
                translateTransform.BeginAnimation(System.Windows.Media.TranslateTransform.YProperty, animY);
            }
            else
            {
                translateTransform.X = gameObject.position.x * Renderer.FieldSize + renderer.offset.x;
                translateTransform.Y = gameObject.position.y * Renderer.FieldSize + renderer.offset.y;
            }
        }

        private void UpdateRotation(bool animated = true)
        {
            renderer.RunningAnimations++;

            //Angles are to left = negative Top is 0
            //000 090 180 270 -> Gameobject
            //090 000 270 180 -> Control
            //
            //000 001 002 003
            //090 -90 090 -90
            //Therefor if even +90 else -90
            //TODO: ACTUAL SOLVE THE PROBLEM

            int angle = 0;
            if ((gameObject.Angle / 90) % 2 == 0) angle = 90;
            else angle = -90;

            System.Windows.Media.Animation.DoubleAnimation animRot = new System.Windows.Media.Animation.DoubleAnimation(gameObject.Angle + angle, TimeSpan.FromSeconds(1));
            animRot.Completed += renderer.EndAnimation;
            rotateTransform.BeginAnimation(System.Windows.Media.RotateTransform.AngleProperty, animRot);
        }

        public void Update(bool forced = false)
        {
            if (gameObject.updated || forced)
            {
                if (gameObject is ImageEntity imageObject)
                {
                    if (imageObject.updateImage)
                    {
                        ((System.Windows.Controls.Image)uIElement).Source = Renderer.BitmapToImageSource(new System.Drawing.Bitmap(imageObject.CurrentImage()));
                        imageObject.updateImage = false;
                    }
                }

                if (gameObject.animate)
                {
                    if (gameObject.animationType == AnimationType.movement) UpdatePosition();
                    else if (gameObject.animationType == AnimationType.rotation) UpdateRotation();
                    gameObject.animate = false;
                }
                else
                {
                    UpdatePosition(animated: false);
                }

                gameObject.updated = false;
            }
        }
    }
}