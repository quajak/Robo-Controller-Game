using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LanguageParser
{
    public class LanguageExecuter
    {
        private string rawInput = "";
        private List<string> commandLines = new List<string>();
        private List<List<string>> words = new List<List<string>>();

        public bool ParseText(string text, out string error)
        {
            try
            {
                rawInput = text;
                commandLines = rawInput.Split('\n').ToList();
                foreach (string line in commandLines)
                {
                    words.Add(line.Split(' ').ToList());
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

        public bool ExectuteFile(out string Error)
        {
            int counter = 0;
            foreach (List<string> line in words)
            {
                counter++;
                if (!ExectuteLine(line, out string error))
                {
                    Error = $"Error in line {counter}: " + error;
                    return false;
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
                case "mov" when parameters.Length == 1:

                    break;

                default:
                    break;
            }
            error = "";
            return true;
        }
    }
}