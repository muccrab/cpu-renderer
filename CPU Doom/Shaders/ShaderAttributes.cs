namespace CPU_Doom.Shaders
{
    // Attributes For Shader Properties.

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public abstract class BasicShaderAttribute : Attribute
    {
        public string Name { get; }
        public BasicShaderAttribute(string name) => Name = name;
    }

    public class InputAttribute : BasicShaderAttribute 
    {
        public int Location { get; set; } // If location = -1, then it will be automatically set based on position in shader. In Fragment shader, position is irrelevant.
        public InputAttribute(string name, int location = -1) : base(name) => Location = location;
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
