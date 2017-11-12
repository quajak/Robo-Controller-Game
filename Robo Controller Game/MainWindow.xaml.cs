using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Robo_Controller_Game
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private GameController gameController;

        public MainWindow()
        {
            InitializeComponent();
            gameController = new GameController((int)GameBoard.Width / 10, (int)GameBoard.Height / 10, GameBoard, CPUProgressBar, this);
        }

        private void GameBoard_Initialized(object sender, EventArgs e)
        {
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            gameController.RunCode(codeView.Text, out string Error);
            if (Error != "") MessageBox.Show(Error);
        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
        }
    }
}