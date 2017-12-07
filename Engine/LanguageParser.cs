using Engine.Programming;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Engine
{
    public class Programm
    {
        private GameController gameController;

        private List<Command> code;
        private Command activeCommand;
        public bool Finished = true;

        public Programm(GameController GameControl)
        {
            gameController = GameControl;
        }

        public bool Parse(string text, out string error)
        {
            error = "";

            //Reset
            code = new List<Command>();
            //Split code
            List<List<string>> rawCode = new List<List<string>>();
            try
            {
                List<string> rawText = text.Replace("\r", "").Split('\n').ToList();
                foreach (string line in rawText)
                {
                    if (line != "") rawCode.Add(line.Split(' ').ToList());
                }
            }
            catch (Exception e)
            {
                error = e.ToString();
                return false;
            };

            //Parse the lines
            //TODO: Keep index and raw line to give user as debug info
            foreach (List<string> line in rawCode)
            {
                Command newCommand = Command.NewCommand(line, gameController, this, out error);
                if (error != "") return false;
                if (code.Count != 0) code.Last().next = newCommand;
                code.Add(newCommand);
            }
            code.Last().next = new EndExecution(this);

            //Get ready for execution
            activeCommand = code.First();
            Finished = false;
            return true;
        }

        public bool ExecuteLine(out string error)
        {
            error = "";
            do
            {
                activeCommand = activeCommand.Run(out error);
            } while (!Finished && activeCommand.CPUTimeMult == 0); //Instanteously do instant actions

            if (error != "") return false;
            return true;
        }

        public void Clear()
        {
            code = new List<Command>();
            Finished = true;
        }

        public static Point GetOffset(int Angle, Point position, GameWorld gameWorld, out MapObject field)
        {
            Point movement;
            switch (Angle)
            {
                case (0) when (position.x != gameWorld.map.GetUpperBound(0)):
                    field = gameWorld.map[position.x + 1, position.y];
                    movement = new Point(1, 0);
                    break;

                case (90) when (position.y != 0):
                    field = gameWorld.map[position.x, position.y - 1];
                    movement = new Point(0, -1);
                    break;

                case (180) when (position.x != 0):
                    field = gameWorld.map[position.x - 1, position.y];
                    movement = new Point(-1, 0);
                    break;

                case (270) when (position.y != gameWorld.map.GetUpperBound(1)):
                    field = gameWorld.map[position.x, position.y + 1];
                    movement = new Point(0, 1);
                    break;

                case int value:
                    field = gameWorld.map[position.x, position.y];
                    movement = new Point(0, 0);
                    break;

                default:
                    throw new Exception("The angle has a wrong value!");
            }
            return movement;
        }
    }
}