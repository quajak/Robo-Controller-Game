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
    public enum EquipmentType { basic, mining, ram };

    public abstract class RobotEquipment
    {
        public string name;
        public readonly string id;
        public Robot robot;
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

        public abstract void SettupRobot();

        protected static ImageSource BtIS(Bitmap image)
        {
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(image.GetHbitmap(),
                IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }
    }

    internal class BasicCPU : RobotEquipment
    {
        public BasicCPU(string ID, Robot robot) : base(ID, "CPU MK 1", robot,
            BtIS(Resources.CPUBasic), -1)
        {
        }

        protected override void UpdateDescription()
        {
            description = "This is the simpelest possible CPU. It only allows basic commands. " +
                "The processor speed is one command a second";
        }

        public override void SettupRobot()
        {
            robot.CPUSpeed = 1000;
        }
    }

    internal class BasicCasing : RobotEquipment
    {
        public BasicCasing(string ID, Robot robot) : base(ID, "Casing MK 1", robot,
            BtIS(Resources.BasicCasingRobot), 5, robotImageSource:
            BtIS(Resources.BasicCasingRobot), Upgrade: new BasicCasingMK2(robot))
        {
        }

        public override void SettupRobot()
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
            BtIS(Resources.Basic2CasingRobot), 5, 80,
            robotImageSource: BtIS(Resources.Basic2CasingRobot))
        {
        }

        public override void SettupRobot()
        {
            robot.health = 20;
        }

        protected override void UpdateDescription()
        {
            description = "This is a cheap and simple casing made out of a mix of iron and aluminium."
                + "It gives the robot 20 heath points";
        }
    }

    #region RAM

    internal abstract class RAM : RobotEquipment
    {
        private int RAMSize;

        public RAM(string Name, string id, Robot robot, ImageSource image, int cost, int _RAMSize,
            RAM upgrade = null, bool IsUpgrade = true) :
            base(id, Name, robot, image, -1, cost, EquipmentType.ram, Upgrade: upgrade, isUpgrade: IsUpgrade)
        {
            RAMSize = _RAMSize;
        }
    }

    internal class BasicRAMMK1 : RAM
    {
        public BasicRAMMK1(Robot robot) : base("Basic RAM MK 1", "BASICRAMMK1", robot,
            BtIS(Resources.BASICRAMMK1), -1, 64, IsUpgrade: false, upgrade: new BasicRAMMK2(robot))
        {
        }

        public override void SettupRobot()
        {
            robot.MAXRAMSize = 64;
        }

        protected override void UpdateDescription()
        {
            description = "This is the simplest RAM. It has 64 bytes of memory.";
        }
    }

    internal class BasicRAMMK2 : RAM
    {
        public BasicRAMMK2(Robot robot) : base("Basic RAM MK 2", "BASICRAMMK2", robot,
            BtIS(Resources.BASICRAMMK2), 50, 128, IsUpgrade: true)
        {
        }

        public override void SettupRobot()
        {
            robot.MAXRAMSize = 128;
        }

        protected override void UpdateDescription()
        {
            description = "This is a slightly improved version of the simplest RAM. It has 128 bytes of memory.";
        }
    }

    #endregion RAM

    #region Drill

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
            BtIS(Resources.DrillBasic), 10, 1, 0.1,
            BtIS(Resources.DrillBasicRobot), false)
        {
        }

        public override void SettupRobot()
        {
        }

        protected override void UpdateDescription()
        {
            description = "This is the simplest drill. It allows the robot to mine, using the mine command. It has a mining speed of 1.";
        }
    }

    #endregion Drill
}