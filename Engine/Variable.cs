using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Programming
{
    public enum VariableType { integer };

    internal abstract class Variable
    {
        public VariableType type;
        public string ID;
    }

    internal class Integer : Variable
    {
        public int value;

        public Integer(int Value, string _ID)
        {
            type = VariableType.integer;
            value = Value;
            ID = _ID;
        }
    }
}