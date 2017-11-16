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
        public RobotPart(string Name, string Descriptor, string Picture)
        {
            InitializeComponent();

            SetName = Name;
            SetDescriptor = Descriptor;
            SetPicturePath = Picture;
        }

        public RobotPart(string Name, string Descriptor, ImageSource Picture)
        {
            InitializeComponent();
            SetName = Name;
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
    }
}