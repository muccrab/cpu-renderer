using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPU_Doom.Shaders
{
    [AttributeUsage(AttributeTargets.Property)]
    public abstract class BasicShaderAttribute : Attribute
    {
        public string Name { get; }
        public BasicShaderAttribute(string name)
        {
            Name = name;
        }
    }


    public class InputAttribute : BasicShaderAttribute 
    {
        public InputAttribute(string name) : base(name) { }
    }

    public class OutputAttribute : BasicShaderAttribute
    {
        public OutputAttribute(string name) : base(name) { }
    }

    public class UniformAttribute : BasicShaderAttribute
    {
        public UniformAttribute(string name) : base(name) { }
    }


}
