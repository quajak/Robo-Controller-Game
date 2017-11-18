using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Robo_Controller_Game
{
    /// <summary>
    /// Interaction logic for RobotPart.xaml
    /// </summary>
    public partial class RobotPart : UserControl
    {
        private string BaseText;
        private bool beingSold = false;
        public bool upgradeable = false;
        private int cost;

        public bool BeingSold
        {
            get { return beingSold; }
            set
            {
                beingSold = value;
                if (beingSold)
                    SetDescriptor = BaseText + $"The price is {cost}.";
                else SetDescriptor = BaseText;
            }
        }

        public RobotPart(string Name, string Descriptor, int Cost, string Picture)
        {
            InitializeComponent();

            cost = Cost;
            SetName = Name;
            BaseText = Descriptor;
            SetDescriptor = Descriptor;
            SetPicturePath = Picture;
        }

        public RobotPart(string Name, string Descriptor, int Cost, ImageSource Picture)
        {
            InitializeComponent();

            cost = Cost;
            SetName = Name;
            BaseText = Descriptor;
            SetDescriptor = Descriptor;
            SetPicture = Picture;
        }

        public string SetName
        {
            get { return title.Content.ToString(); }
            set
            { title.Content = value; }
        }

        public string SetDescriptor
        {
            get { return mainText.Text.ToString(); }
            set
            {
                mainText.Text = value;
            }
        }

        public ImageSource SetPicture
        {
            set
            {
                image.Source = value;
            }
        }

        public string SetPicturePath
        {
            set
            {
                image.Source = new BitmapImage(new Uri(value));
            }
        }

        public event EventHandler Click;

        private void Clicked(object sender, MouseButtonEventArgs e)
        {
            if (Click != null)
                this.Click(this, e);
        }

        private void Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!beingSold) overlay.Visibility = Visibility.Visible;
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!beingSold) overlay.Visibility = Visibility.Hidden;
        }
    }
}