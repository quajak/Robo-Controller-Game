using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public class LanguageExecuter
    {
        private string rawInput = "";
        private List<string> commandLines = new List<string>();
        public List<List<string>> words = new List<List<string>>();
        private GameController gameController;
        public List<string> actions = new List<string>();

        public LanguageExecuter(GameController controller)
        {
            gameController = controller;
        }

        public bool ParseText(string text, out string error)
        {
            words = new List<List<string>>();
            try
            {
                rawInput = text;
                commandLines = rawInput.Replace("\r", "").Split('\n').ToList();
                foreach (string line in commandLines)
                {
                    if (line != "") words.Add(line.Split(' ').ToList());
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
            if (!ExectuteLine(words[0], out error))
            {
                return false;
            };
            return true;
        }

        public bool ExecuteFile(out string Error, bool wholeFile)
        {
            if (!wholeFile) return ExecuteFile(out Error);
            int counter = 0;
            foreach (List<string> line in words)
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

                default:
                    break;
            }
            words.RemoveAt(0);
            error = "";
            return true;
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
                    break;
            }
            error = "";
            return true;
        }
    }
}