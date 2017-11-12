using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace Engine
{
    public class GameController
    {
        public GameWorld gameWorld;
        public Robot robot;
        public Renderer renderer;
        private int entityCounter = 0;
        private LanguageExecuter languageParser;

        private DispatcherTimer commandTimer;
        private int timeElapsed = 0;
        private ProgressBar CPUprogressBar;

        public GameController(int width, int height, Canvas GameBoard, ProgressBar CPUProgressBar, Window window)
        {
            CPUprogressBar = CPUProgressBar;
            Point start = new Point(0, 0);
            languageParser = new LanguageExecuter(this);
            gameWorld = new GameWorld(width, height, start);
            robot = new Robot(entityCounter++, start, gameWorld);
            renderer = new Renderer(GameBoard, gameWorld.map.Cast<GameObject>().ToList());
            renderer.AddEntity(robot);

            commandTimer = new DispatcherTimer
            {
                Interval = new TimeSpan(1000)
            };
            EventHandler eventHandler = new EventHandler(HandleCommandTime);
            commandTimer.Tick += eventHandler;
            commandTimer.Start();
            SettupRobotInfo();
        }

        private void SettupRobotInfo()
        {
            CPUprogressBar.Maximum = robot.speed;
            return;
        }

        private void HandleCommandTime(object obj, EventArgs args)
        {
            if (languageParser.words.Count + languageParser.actions.Count == 0) return;
            timeElapsed += 10;
            string error = "";
            if (timeElapsed >= robot.speed)
            {
                timeElapsed = 0;
                if (languageParser.actions.Count != 0)
                {
                    languageParser.ExecuteAction(out error);
                }
                else
                {
                    languageParser.ExecuteFile(out error);
                }
                renderer.Update();
            }
            if (error != "")
            {
                MessageBox.Show(error);
            }
            try
            {
                CPUprogressBar.Value = timeElapsed;
            }
            catch (Exception e)
            {
            }
            return;
        }

        public bool RunCode(string code, out string Error)
        {
            if (!languageParser.ParseText(code, out string error))
            {
                Error = "Input is incorrect: " + error;
                return false;
            }
            Error = "";
            return true;
        }
    }
}