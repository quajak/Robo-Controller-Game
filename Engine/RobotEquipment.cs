using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Engine
{
    public abstract class RobotEquipment
    {
        public string name;
        public readonly string id;
        protected Robot robot;
        protected string description;
        public ImageSource image;
        public readonly int drawingLevel;

        public string Description
        {
            get
            {
                UpdateDescription();
                return description;
            }
        }

        public RobotEquipment(string ID, string Name, Robot Robot, ImageSource imageSource, int DrawingLevel)
        {
            image = imageSource;
            id = ID;
            name = Name;
            robot = Robot;
            drawingLevel = DrawingLevel;
        }

        protected abstract void UpdateDescription();

        protected abstract void SettupRobot();

        protected static ImageSource BitmapToImageSource(Bitmap image)
        {
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(image.GetHbitmap(),
                IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }
    }

    internal class BasicCPU : RobotEquipment
    {
        public BasicCPU(string ID, Robot robot) : base(ID, "CPU MK 1", robot,
            BitmapToImageSource(Resources.CPUBasic), -1)
        {
        }

        protected override void UpdateDescription()
        {
            description = "This is the simpelest possible CPU. It only allows basic commands. " +
                "The processor speed is one command a second";
        }

        protected override void SettupRobot()
        {
            robot.CPUSpeed = 1000;
        }
    }

    internal class BasicCasing : RobotEquipment
    {
        public BasicCasing(string ID, Robot robot) : base(ID, "Casing MK 1", robot,
            BitmapToImageSource(Resources.BasicCasing), 0)
        {
        }

        protected override void SettupRobot()
        {
            robot.health = 10;
        }

        protected override void UpdateDescription()
        {
            description = @"This is a very cheap and simple casing made out of aluminium."
                + "It gives the robot 10 heath points";
        }
    }
}