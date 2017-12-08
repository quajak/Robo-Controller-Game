using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Programming
{
    internal abstract class Command
    {
        public bool setNext = true;
        public int CPUTimeMult = 1;
        public Command next;
        protected readonly GameController gameController;
        protected readonly Programm programm;

        public Command(GameController _gameController, Programm _programm)
        {
            gameController = _gameController;
            programm = _programm;
        }

        public static Command NewCommand(List<string> command, GameController _gameController, Programm _programm, out string error)
        {
            error = "";
            List<string> parameters = command.Where((s, i) => i != 0).ToList();
            switch (command[0])
            {
                case "mine":
                    return new Mine(parameters, _gameController, _programm, out error);

                case "mv":
                case "mov":
                    return new Move(parameters, _gameController, _programm, out error);

                case "rot":
                    return new Rotate(parameters, _gameController, _programm, out error);

                case "lbl":
                    return new Label(parameters, _gameController, _programm, out error);

                case "gt":
                case "goto":
                    return new GOTO(parameters, _gameController, _programm, out error);

                default:
                    throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }

        public abstract Command Run(out string error);
    }

    internal class Label : Command
    {
        public string ID;

        public Label(List<string> parameters, GameController _gameController, Programm _programm, out string error) : base(_gameController, _programm)
        {
            error = "";
            if (parameters.Count != 1)
            {
                error = "Syntax Error: Need 1 Parameter!";
                return;
            }

            if (programm.labels.Exists(l => l.ID == parameters[0]))
            {
                error = $"This label already existst!{parameters[0]}";
                return;
            }

            ID = parameters[0];
            programm.labels.Add(this);

            CPUTimeMult = 0;
        }

        public override Command Run(out string error)
        {
            error = "";
            return next;
        }
    }

    internal class GOTO : Command
    {
        public GOTO(List<string> parameters, GameController _gameController, Programm _programm, out string error)
            : base(_gameController, _programm)
        {
            error = "";
            if (parameters.Count != 1)
            {
                error = "Syntax Error: Need 1 paramter!";
                return;
            }

            if (!programm.labels.Exists(l => l.ID == parameters[0]))
            {
                error = $"No label with the name {parameters[0]} exists!";
                return;
            }

            setNext = false;
            CPUTimeMult = 0;
            next = programm.labels.First(l => l.ID == parameters[0]);
        }

        public override Command Run(out string error)
        {
            error = "";
            return next;
        }
    }

    internal class Rotate : Command
    {
        private int angle;

        public Rotate(List<string> parameters, GameController _gameController, Programm _programm, out string error) : base(_gameController, _programm)
        {
            error = "";
            if (parameters.Count != 1)
            {
                error = "A wrong number of paramters have been given!";
                return;
            }
            if (parameters[0] == "left") angle = 90;
            else if (parameters[0] == "right") angle = -90;
            if (angle == 0 && !Int32.TryParse(parameters[0], out angle)) error = $"The first parameter was not a valid term! - {parameters[0]}";
        }

        public override Command Run(out string error)
        {
            error = "";
            gameController.robot.Angle += angle;
            gameController.robot.updated = true;
            gameController.robot.animate = true;
            gameController.robot.animationType = AnimationType.rotation;
            return next;
        }
    }

    internal class Move : Command
    {
        private int counter = 0, max;

        public Move(List<string> parameters, GameController _gameController, Programm _programm, out string error) : base(_gameController, _programm)
        {
            error = "";

            if (parameters.Count != 1)
            {
                error = $"Expected 1 paramter, got {parameters.Count}!";
                return;
            }
            if (!Int32.TryParse(parameters[0], out max))
            {
                error = $"The first parameter is not a number! ({parameters[0]})";
                return;
            }
            if (max <= 0) error = "The distance must be a non-negative and non-zero number!";

            if (error != "") return;
        }

        public override Command Run(out string error)
        {
            gameController.robot.EnterField(gameController.robot);
            gameController.robot.updated = true;
            gameController.robot.animate = true;
            gameController.robot.animationType = AnimationType.movement;

            error = "";
            counter++;
            if (counter == max) return next;
            return this;
        }
    }

    internal class Mine : Command
    {
        public Mine(List<string> parameters, GameController _gameController, Programm _programm, out string error) : base(_gameController, _programm)
        {
            error = "";
            if (parameters.Count != 0) error = "Too many parameters! Invalid line!";

            if (gameController.activeEquipment.Find(e => e.type == EquipmentType.mining) == null) error = "You do not have any equipment that can mine!";

            if (error != "") return;
        }

        public override Command Run(out string error)
        {
            if (next == null) throw new Exception("The next command has not been set!");

            error = "";
            Point direction = Programm.GetOffset(gameController.robot.Angle, gameController.robot.position, gameController.gameWorld, out MapObject field);
            if (field == null) throw new Exception("Something went wrong!");
            if (direction == new Point(0, 0) || !field.mineable)
            {
                error = "The field is equal to null!";
            };
            int x = field.position.x / GameWorld.fieldSize;
            int y = field.position.y / GameWorld.fieldSize;
            //mine field
            gameController.renderer.RemoveEntity(gameController.gameWorld.map[x, y]);
            gameController.gameWorld.map[x, y] = new Space(field.id, field.position) { updated = true };
            gameController.renderer.AddEntity(gameController.gameWorld.map[x, y], Rendering.RenderObjectType.rectangle);
            return next;
        }
    }

    internal class EndExecution : Command
    {
        public EndExecution(Programm programm) : base(null, programm)
        {
            CPUTimeMult = 0;
        }

        public override Command Run(out string error)
        {
            error = "";
            programm.Finished = true;
            return null;
        }
    }
}