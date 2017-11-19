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
    public enum EquipmentType { basic, mining };

    public abstract class RobotEquipment
    {
        public string name;
        public readonly string id;
        protected Robot robot;
        protected string description;
        public ImageSource image;
        public readonly int drawingLevel;
        public int price;
        public EquipmentType type;
        public ImageSource robotImage;
        public bool IsUpgrade;

        public RobotEquipment upgrade; //TODO: Allow multiple robot equipment upgrae possibilites?

        public string Description
        {
            get
            {
                UpdateDescription();
                return description;
            }
        }

        public RobotEquipment(string ID, string Name, Robot Robot, ImageSource imageSource,
            int DrawingLevel, int Price = 0, EquipmentType Type = EquipmentType.basic,
            ImageSource robotImageSource = null, RobotEquipment Upgrade = null, bool isUpgrade = true)
        {
            type = Type;
            price = Price;
            image = imageSource;
            id = ID;
            name = Name;
            robot = Robot;
            robotImage = robotImageSource;
            drawingLevel = DrawingLevel;
            IsUpgrade = isUpgrade;
            if (Upgrade != null) upgrade = Upgrade;
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
            BitmapToImageSource(Resources.BasicCasingRobot), 5, robotImageSource:
            BitmapToImageSource(Resources.BasicCasingRobot), Upgrade: new BasicCasingMK2(robot))
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

    internal class BasicCasingMK2 : RobotEquipment
    {
        public BasicCasingMK2(Robot robot) : base("BasicCasingMK2", "Basic Casing MK 2", robot,
            BitmapToImageSource(Resources.Basic2CasingRobot), 5, 80,
            robotImageSource: BitmapToImageSource(Resources.Basic2CasingRobot))
        {
        }

        protected override void SettupRobot()
        {
            robot.health = 20;
        }

        protected override void UpdateDescription()
        {
            description = "This is a cheap and simple casing made out of a mix of iron and aluminium."
                + "It gives the robot 20 heath points";
        }
    }

    internal abstract class Drill : RobotEquipment
    {
        private int miningSpeed;
        private double efficency;

        public Drill(string name, string id, Robot robot, ImageSource image, int cost, int MiningSpeed, double Efficiency, ImageSource robotImage, bool IsUpgrade)
            : base(id, name, robot, image, 4, cost, EquipmentType.mining, robotImageSource: robotImage,
                  isUpgrade: IsUpgrade)
        {
        }
    }

    internal class BasicDrill : Drill
    {
        public BasicDrill(Robot robot) : base("Drill MK 1", "BasicDrill", robot,
            BitmapToImageSource(Resources.DrillBasic), 10, 1, 0.1,
            BitmapToImageSource(Resources.DrillBasicRobot), false)
        {
        }

        protected override void SettupRobot()
        {
        }

        protected override void UpdateDescription()
        {
            description = "This is the simplest drill. It allows the robot to mine, using the mine command. It has a mining speed of 1.";
        }
    }
}