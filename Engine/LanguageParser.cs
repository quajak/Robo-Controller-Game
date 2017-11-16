using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Engine
{
    public class LanguageExecuter
    {
        private string rawInput = "";
        private List<string> commandLines = new List<string>();
        public List<List<string>> commands = new List<List<string>>();
        private GameController gameController;
        public List<string> actions = new List<string>();

        public LanguageExecuter(GameController controller)
        {
            gameController = controller;
        }

        public bool ParseText(string text, out string error)
        {
            commands = new List<List<string>>();
            try
            {
                rawInput = text;
                commandLines = rawInput.Replace("\r", "").Split('\n').ToList();
                foreach (string line in commandLines)
                {
                    if (line != "") commands.Add(line.Split(' ').ToList());
                }
                error = "";
                return true;
            }
            catch (Exception e)
            {
                error = e.ToString();
                return false;
            };
        }

        public bool ExecuteFile(out string error)
        {
            if (!ExectuteLine(commands[0], out error))
            {
                return false;
            };
            return true;
        }

        public bool ExecuteFile(out string Error, bool wholeFile)
        {
            if (!wholeFile) return ExecuteFile(out Error);
            int counter = 0;
            foreach (List<string> line in commands)
            {
                counter++;
                if (!ExectuteLine(line, out string error))
                {
                    Error = $"Error in line {counter}: " + error;
                    return false;
                }
                else
                {
                    gameController.renderer.Update();
                }
            }
            Error = "";
            return true;
        }

        private bool ExectuteLine(List<string> line, out string error)
        {
            string[] parameters = line.Where((l, i) => i != 0).ToArray();
            switch (line[0])
            {
                case "mv" when parameters.Length == 1:
                case "mov" when parameters.Length == 1:
                    if (Int32.TryParse(parameters[0], out int distance))
                    {
                        if (distance > 0)
                        {
                            for (int i = 0; i < distance; i++)
                            {
                                actions.Add("mov");
                            }
                        }
                        else
                        {
                            error = "The distance moved must be larger than 0!";
                            return false;
                        }
                    }
                    else
                    {
                        error = "The first paramter was not a number!";
                        return false;
                    }
                    ExecuteAction(out error);
                    break;

                case "rot" when parameters.Length == 1:
                    int angle = 0;
                    if (parameters[0].ToLower() == "left") angle = 90;
                    else if (parameters[0].ToLower() == "right") angle = 270;
                    if (angle != 0 || Int32.TryParse(parameters[0], out angle))
                    {
                        gameController.robot.Angle += angle;
                    }
                    else
                    {
                        error = "The first paramter was not a number!";
                        return false;
                    }
                    break;

                case "mine" when parameters.Length == 0:
                    //Check if player is able to mine
                    if (gameController.activeEquipment.Find(e => e.type == EquipmentType.mining) != null)
                    {
                        Vector direction = GetOffset(gameController.robot.Angle, gameController.robot.position, gameController.gameWorld, out MapObject field);
                        if (field == null) throw new Exception("Something went wrong!");
                        if (!field.mineable) break;
                        int x = (int)(field.position.X / GameWorld.fieldSize);
                        int y = (int)(field.position.Y / GameWorld.fieldSize);
                        //mine field
                        gameController.renderer.RemoveEntity(gameController.gameWorld.map[x, y]);
                        gameController.gameWorld.map[x, y] = new Space(field.id, field.position) { updated = true };
                        gameController.renderer.AddEntity(gameController.gameWorld.map[x, y]);
                    }
                    else
                    {
                        error = "You do not have any equpiment that can mine!";
                        return false;
                    }

                    break;

                default:
                    break;
            }
            commands.RemoveAt(0);
            error = "";
            return true;
        }

        public static Vector GetOffset(int Angle, Point position, GameWorld gameWorld, out MapObject field)
        {
            Vector movement;
            switch (Angle)
            {
                case (0) when (position.X != gameWorld.map.GetUpperBound(0)):
                    field = gameWorld.map[(int)position.X + 1, (int)position.Y];
                    movement = new Vector(1, 0);
                    break;

                case (90) when (position.Y != 0):
                    field = gameWorld.map[(int)position.X, (int)position.Y - 1];
                    movement = new Vector(0, -1);
                    break;

                case (180) when (position.X != 0):
                    field = gameWorld.map[(int)position.X - 1, (int)position.Y];
                    movement = new Vector(-1, 0);
                    break;

                case (270) when (position.Y != gameWorld.map.GetUpperBound(1)):
                    field = gameWorld.map[(int)position.X, (int)position.Y + 1];
                    movement = new Vector(0, 1);
                    break;

                default:
                    throw new Exception("The angle has a wrong value!");
            }
            return movement;
        }

        public bool ExecuteAction(out string error)
        {
            switch (actions[0])
            {
                case "mov":
                    actions.RemoveAt(0);
                    gameController.robot.EnterField(gameController.robot);
                    break;

                case "rot":

                    break;

                default:
                    throw new Exception("This action is not defined!" + actions[0]);
            }
            error = "";
            return true;
        }
    }
}