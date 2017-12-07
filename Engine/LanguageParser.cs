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
        private List<List<string>> code;
        private int ExecutionIndex = 0;
        public bool Finished = true;

        public Programm(GameController GameControl)
        {
            gameController = GameControl;
        }

        public bool Parse(string text, out string error)
        {
            error = "";

            //Reset
            code = new List<List<string>>();
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
            int index = 0;
            foreach (List<string> line in rawCode)
            {
                code.Add(new List<string>());
                List<string> parameters = line.Where((l, i) => i != 0).ToList();
                switch (line[0])
                {
                    case "mv" when parameters.Count == 1:
                    case "mov" when parameters.Count == 1:
                        if (Int32.TryParse(parameters[0], out int distance))
                        {
                            if (distance > 0)
                            {
                                for (int i = 0; i < distance; i++)
                                {
                                    if (i != 0) code.Add(new List<string>());
                                    code[index].Add("mov");
                                    if (i + 1 < distance) index++;
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
                        break;

                    case "rot" when parameters.Count == 1:
                        int angle = 0;
                        if (parameters[0].ToLower() == "left") angle = 90;
                        else if (parameters[0].ToLower() == "right") angle = -90;
                        if (angle != 0 || Int32.TryParse(parameters[0], out angle))
                        {
                            code[index].Add("rot");
                            code[index].Add(angle.ToString());
                        }
                        else
                        {
                            error = "The first paramter was not a number!";
                            return false;
                        }
                        break;

                    case "mine" when parameters.Count == 0:
                        //Check if player is able to mine
                        if (gameController.activeEquipment.Find(e => e.type == EquipmentType.mining) != null)
                        {
                            code[index].Add("mine");
                        }
                        else
                        {
                            error = "You do not have any equpiment that can mine!";
                            return false;
                        }

                        break;

                    default:
                        string _line = "";
                        line.ForEach(l => _line += l + " ");
                        error = "Syntax Error: " + _line;
                        return false;
                }
                index++;
            }
            //Get ready for execution
            Finished = false;
            ExecutionIndex = 0;
            return true;
        }

        public bool ExecuteLine(out string error)
        {
            error = "";

            List<string> line = code[ExecutionIndex];
            string[] parameters = line.Where((l, i) => i != 0).ToArray();
            ExecutionIndex++;
            if (ExecutionIndex >= code.Count) Finished = true;

            switch (line[0])
            {
                case "mov" when parameters.Length == 0:

                    gameController.robot.EnterField(gameController.robot);
                    gameController.robot.updated = true;
                    gameController.robot.animate = true;
                    gameController.robot.animationType = AnimationType.movement;

                    break;

                case "rot" when parameters.Length == 1:
                    Int32.TryParse(parameters[0], out int angle);
                    gameController.robot.Angle += angle;
                    gameController.robot.updated = true;
                    gameController.robot.animate = true;
                    gameController.robot.animationType = AnimationType.rotation;
                    break;

                case "mine" when parameters.Length == 0:
                    //Check if player is able to mine
                    if (gameController.activeEquipment.Find(e => e.type == EquipmentType.mining) != null)
                    {
                        Point direction = GetOffset(gameController.robot.Angle, gameController.robot.position, gameController.gameWorld, out MapObject field);
                        if (field == null) throw new Exception("Something went wrong!");
                        if (direction == new Point(0, 0) || !field.mineable) break;
                        int x = (int)(field.position.x / GameWorld.fieldSize);
                        int y = (int)(field.position.y / GameWorld.fieldSize);
                        //mine field
                        gameController.renderer.RemoveEntity(gameController.gameWorld.map[x, y]);
                        gameController.gameWorld.map[x, y] = new Space(field.id, field.position) { updated = true };
                        gameController.renderer.AddEntity(gameController.gameWorld.map[x, y], Rendering.RenderObjectType.rectangle);
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
            return true;
        }

        public void Clear()
        {
            code = new List<List<string>>();
            Finished = true;
            ExecutionIndex = 0;
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