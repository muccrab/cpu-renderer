namespace CPU_Doom.Shaders
{
    /// <summary>
    /// Abstract Attribute used to mark shader parameters.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public abstract class BasicShaderAttribute : Attribute
    {
        /// <summary>
        /// The name of the shader property.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Base constructor for any shader attribute.
        /// </summary>
        /// <param name="name">The name of the property this attribute is assigned to.</param>
        public BasicShaderAttribute(string name) => Name = name;
    }

    /// <summary>
    /// Attribute used to mark shader inputs.
    /// </summary>
    public class InputAttribute : BasicShaderAttribute 
    {
        /// <summary>
        /// The location of the input in the shader. If -1, it will be auto-assigned.
        /// </summary>
        public int Location { get; set; } // If location = -1, then it will be automatically set based on position in shader. In Fragment shader, position is irrelevant.
        
        /// <summary>
        /// Constructor for InputAttribute.
        /// </summary>
        /// <param name="name">Name of the shader input.</param>
        /// <param name="location">Optional location, defaults to -1 (auto-assign).</param>
        public InputAttribute(string name, int location = -1) : base(name) => Location = location;
    }

    /// <summary>
    /// Attribute used to mark shader outputs.
    /// </summary>
    public class OutputAttribute : BasicShaderAttribute
    {
        /// <summary>
        /// Constructor for OutputAttribute.
        /// </summary>
        /// <param name="name">Name of the shader output.</param>
        public OutputAttribute(string name) : base(name) { }
    }

    /// <summary>
    /// Attribute used to mark shader uniforms.
    /// </summary>
    public class UniformAttribute : BasicShaderAttribute
    {
        /// <summary>
        /// Constructor for UniformAttribute.
        /// </summary>
        /// <param name="name">Name of the shader uniform.</param>
        public UniformAttribute(string name) : base(name) { }
    }
}
