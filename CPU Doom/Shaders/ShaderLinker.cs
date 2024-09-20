using System.Reflection;
using System.Text;

namespace CPU_Doom.Shaders
{
    /// <summary>
    /// Helps in linking vertex and fragment shaders by resolving and matching input and output fields, as well as uniforms.
    /// </summary>
    internal class ShaderLinker
    {
        /// <summary>
        /// Gets the list of input properties of the vertex shader.
        /// </summary>
        public List<FieldInfo> VertexInputs => _vertexInputs; // All Input Properties of Vertex Shader

        /// <summary>
        /// Gets a dictionary of linked variables from vertex shader outputs to fragment shader inputs.
        /// </summary>
        public Dictionary<string, ShaderVariable> LinkedVariables => _linkedVariables; // Linked Vertex Outputs -> Fragment Inputs

        /// <summary>
        /// Gets the output property of the fragment shader.
        /// </summary>
        public FieldInfo FragmentOutput => _fragmentOutput; // Fragment Output Property

        /// <summary>
        /// Gets a dictionary of shader uniforms.
        /// </summary>
        public Dictionary<string, ShaderUniform> Uniforms => _uniforms; // Shader Uniforms

        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderLinker"/> class and links shaders.
        /// </summary>
        /// <param name="vertexType">The type of the vertex shader.</param>
        /// <param name="fragmentType">The type of the fragment shader.</param>
        /// <exception cref="ArgumentNullException">Thrown if the fragment shader does not contain an output attribute.</exception>
        public ShaderLinker(Type vertexType, Type fragmentType)
        {
            _vertexType = vertexType;
            _fragmentType = fragmentType;
            LinkShaders();
            if (_fragmentOutput == null)
            {
                WindowStatic.Logger.LogError("Fragment Shader must contain a output.");
                throw new ArgumentNullException("Fragment Shader must contain a output."); // After linking there should be an output assigned.
            }
            WindowStatic.Logger.LogSuccess($"Shaders {vertexType} and {fragmentType} have been successfully linked");
        }

        private void LinkShaders()
        {
            LinkVariables();
            LinkUniforms();
        }
        private void LinkVariables() //TODO: SPlit this method
        {
            // Get Input and Output Variables
            GetInputOutputFields(_vertexType, out IEnumerable<FieldInfo> vertexInputs, out IEnumerable<FieldInfo> vertexOutputs);
            GetInputOutputFields(_fragmentType, out IEnumerable<FieldInfo> fragmentInputs, out IEnumerable<FieldInfo> fragmentOutputs);

            // Process Vertex Inputs
            SortedDictionary<int, FieldInfo> specifiedFields = new SortedDictionary<int, FieldInfo>();
            Queue<FieldInfo> unspecifiedFields = new Queue<FieldInfo>();
            foreach (FieldInfo field in vertexInputs)
            {
                var attribute = field.GetCustomAttribute<InputAttribute>();
                if (attribute == null) continue; //This check is pointless, but it needs to be here so C# compiler won't scream at me with warnings

                if (attribute.Location == -1) unspecifiedFields.Enqueue(field);
                else
                {
                    if (specifiedFields.ContainsKey(attribute.Location))
                    {
                        WindowStatic.Logger.LogError("Shader Linking Error - Vertex Input Fields Error - Contains 2 Inputs with the same location");
                        return; // TODO: Exception
                    }
                    specifiedFields.Add(attribute.Location, field);
                }
            }
            _vertexInputs = specifiedFields.Values.ToList();
            int unspecifiedCount = unspecifiedFields.Count;
            for (int i = 0; i < unspecifiedCount; i++)
            {
                FieldInfo input = unspecifiedFields.Dequeue();
                _vertexInputs.Add(input);
            }


            // Join Vertex Outputs and Fragment Inputs.
            // Inner Join of Vertex Outputs and Fragment Inputs
            var verOutFragInTable = from verOut in vertexOutputs
                                    join fragIn in fragmentInputs on verOut.GetCustomAttribute<OutputAttribute>()?.Name equals fragIn.GetCustomAttribute<InputAttribute>()?.Name
                                    select (verOut, fragIn);

            foreach (var verInFragOut in verOutFragInTable)
            {
                FieldInfo verOut = verInFragOut.verOut;
                FieldInfo fragIn = verInFragOut.fragIn;

                var verOutAt = verInFragOut.verOut.GetCustomAttribute<OutputAttribute>();
                var fragInAt = verInFragOut.fragIn.GetCustomAttribute<InputAttribute>();
                if (verOutAt == null || fragInAt == null) continue; //Again pointless, but I won't get warnings
                if (!verOut.FieldType.IsAssignableTo(fragIn.FieldType))
                {
                    StringBuilder builder = new StringBuilder()
                        .Append("Shader Linking Error - Vertex Output: ")
                        .Append(verOutAt.Name).Append(" ( ").Append(verOut.Name).Append(" ) ")
                        .Append("is not compatible with Fragment Input: ")
                        .Append(fragInAt.Name).Append(" ( ").Append(verOut.Name).Append(" )");


                    WindowStatic.Logger.LogWarn(builder.ToString()); 
                    continue; 
                } 

                var shaderVar = new ShaderVariable(verOut, fragIn, SupportsFiltering(verOut.FieldType));

                if (_linkedVariables.ContainsKey(verOutAt.Name)) continue; // Logger + Consider exception.
                _linkedVariables[verOutAt.Name] = shaderVar;
            }

            // Process Fragment Output
            if (!fragmentOutputs.HasExactlyOneElement()) 
            {
                WindowStatic.Logger.LogError("Shader Linking Error - Fragment Output Error - Fragment Shader must have at least and no more than one output attribute");
                return; // TODO: Exception
            }
            
            var fragOut = fragmentOutputs.First();
            if (!fragOut.FieldType.GetCustomAttributes(typeof(SerializableAttribute), true).Any())
            {
                WindowStatic.Logger.LogError("Shader Linking Error - Fragment Output Error - Output Attribute must be serializable");
                return; // TODO: Exception
            }
            _fragmentOutput = fragOut;
        }


        private void LinkUniforms()
        {
            GetUniforms(_vertexType, out var vertexUniforms);
            GetUniforms(_fragmentType, out var fragmentUniforms);

            var verFragUniforms = vertexUniforms.OuterJoin(fragmentUniforms, (ver, frag) => {
                var verAt = ver.GetCustomAttribute<UniformAttribute>();
                var fragAt = frag.GetCustomAttribute<UniformAttribute>();
                if (verAt == null || fragAt == null) return false;
                return verAt.Name == fragAt.Name;
            });

            foreach (var uniform in verFragUniforms)
            {
                ShaderUniform? linkedUniform = ShaderUniform.LinkUniform(uniform.Item1, uniform.Item2);
                if (linkedUniform == null)
                {
                    FieldInfo? uniformInfo = uniform.Item1 ?? uniform.Item2;
                    UniformAttribute? uniformAttribute = uniformInfo?.GetCustomAttribute<UniformAttribute>();
                    string errormessage = "";
                    if (uniformAttribute != null && uniformInfo != null) errormessage = $"Uniform { uniformAttribute.Name } (  { uniformInfo.Name }) couldn't be linked";  
                    WindowStatic.Logger.LogWarn($"Shader Linking Error - Uniform Linking Error - {errormessage}");
                    continue;
                }
                var unAt = uniform.Item1?.GetCustomAttribute<UniformAttribute>() ?? 
                           uniform.Item2?.GetCustomAttribute<UniformAttribute>();
                if (unAt == null) continue; // Never gonna happen, but I'm going to leave this here so compiller is satisfied
                _uniforms.Add(unAt.Name, linkedUniform);
            }
        }
        private void GetInputOutputFields(Type type, out IEnumerable<FieldInfo> inputs, out IEnumerable<FieldInfo> outputs) 
        {
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            inputs = (from field in fields where (field.GetCustomAttribute(typeof(InputAttribute)) != null) && field.FieldType.IsValueType select field);
            outputs = (from field in fields where (field.GetCustomAttribute(typeof(OutputAttribute)) != null) && field.FieldType.IsValueType select field);
        }
        private void GetUniforms(Type type, out IEnumerable<FieldInfo> uniforms)
        {
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);
            uniforms = (from field in fields where field.GetCustomAttribute(typeof(UniformAttribute)) != null && field.FieldType.IsValueType select field).DistinctBy(
                field =>
                {
                    var at = field.GetCustomAttribute<UniformAttribute>();
                    if (at != null) return at.Name;
                    return ""; // Will never get here because of the code above.
                }
            );
        }

        // Checks if type can be filtered by the shader program
        private static bool SupportsFiltering(Type type) =>
            type.GetMethod("op_Multiply", BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(float), type }, null)?.ReturnType == type &&
            type.GetMethod("op_Addition", BindingFlags.Static | BindingFlags.Public, null, new[] { type, type }, null)?.ReturnType == type;


        /// <summary>
        /// Sets the value of a shader uniform.
        /// </summary>
        /// <param name="name">The name of the uniform.</param>
        /// <param name="value">The value to set for the uniform.</param>
        public void SetUniform(string name, object value)
        {
            if (_uniforms.ContainsKey(name))
                _uniforms[name].SetUniform(value);
            else
                WindowStatic.Logger.LogWarn($"Shader Uniform Error - Uniform {name} is not found. Please make sure the uniform exists, or is static");
        }

        private Type _vertexType, _fragmentType;
        private List<FieldInfo> _vertexInputs = new List<FieldInfo>(); // All Input Properties of Vertex Shader
        private Dictionary<string, ShaderVariable> _linkedVariables = new Dictionary<string, ShaderVariable>(); // Dictionary for vertexOut/fragmentIn.
        private FieldInfo _fragmentOutput; // Fragment Output Property
        private Dictionary<string, ShaderUniform> _uniforms = new Dictionary<string, ShaderUniform>(); // Shader Uniforms

        /// <summary>
        /// Represents a variable that links vertex shader output fields to fragment shader input fields.
        /// </summary>
        public class ShaderVariable
        {
            /// <summary>
            /// Gets the vertex field info.
            /// </summary>
            public FieldInfo VertexField { get; private init; }

            /// <summary>
            /// Gets the fragment field info.
            /// </summary>
            public FieldInfo FragmentField { get; private init; }

            /// <summary>
            /// Gets or sets a value indicating whether filtering is enabled for this shader variable.
            /// </summary>
            public bool FileringEnabled { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="ShaderVariable"/> class.
            /// </summary>
            /// <param name="vertexField">The vertex shader output field.</param>
            /// <param name="fragmentField">The fragment shader input field.</param>
            /// <param name="fileringEnabled">A value indicating whether filtering is enabled.</param>
            public ShaderVariable(FieldInfo vertexField, FieldInfo fragmentField, bool fileringEnabled)
            {
                VertexField = vertexField;
                FragmentField = fragmentField;
                FileringEnabled = fileringEnabled;
            }
        }

        /// <summary>
        /// Represents a shader uniform that can be set with values.
        /// </summary>
        public abstract class ShaderUniform
        {
            /// <summary>
            /// Sets the value of the shader uniform.
            /// </summary>
            /// <param name="value">The value to set for the uniform.</param>
            public abstract void SetUniform(object value);

            /// <summary>
            /// Links vertex and fragment shader fields to create a shader uniform.
            /// </summary>
            /// <param name="vertexField">The vertex shader field info.</param>
            /// <param name="fragmentField">The fragment shader field info.</param>
            /// <returns>A <see cref="ShaderUniform"/> instance, or null if linking failed.</returns>
            public static ShaderUniform? LinkUniform(FieldInfo? vertexField, FieldInfo? fragmentField)
            {
                Type? uniformType = TryGetType(vertexField, fragmentField); // Get's Type from vertexField
                if (uniformType == null) return null;

                Type constructedType; // Shader Uniform For Specific Type
                if (!_constructedUniformCache.ContainsKey(uniformType))
                {
                    Type genericuniform = typeof(ShaderUniform<>);
                    constructedType = genericuniform.MakeGenericType(uniformType);
                    _constructedUniformCache.Add(uniformType, constructedType);
                }
                else constructedType = _constructedUniformCache[uniformType];
                ShaderUniform? uniform = (ShaderUniform?)Activator.CreateInstance(constructedType, vertexField, fragmentField); // Creates Uniform Object for specific Type
                return uniform;
            }
            private static Type? TryGetType(FieldInfo? vertexField, FieldInfo? fragmentField) 
            {
                Type? vertexType = vertexField?.FieldType ?? null;
                Type? fragmentType = fragmentField?.FieldType ?? null;
                bool vertexNull = vertexType == null;
                bool fragmentNull = fragmentType == null;
                if (vertexNull && fragmentNull) { return null; }
                else if (vertexNull) { return fragmentType; }
                else if (fragmentNull) { return vertexType; }
                else if (vertexType == fragmentType) { return vertexType; }
                WindowStatic.Logger.LogWarn("Couldn't Find the type for the uniform");
                return null;
            }
          
            static Dictionary<Type, Type> _constructedUniformCache = new Dictionary<Type, Type>();
        }

        // Shader Uniform For Specific Type
        private class ShaderUniform<UniformType> : ShaderUniform where UniformType : struct
        {
            public FieldInfo? VertexField { get; private set; }
            public FieldInfo? FragmentField { get; private set; }
            public ShaderUniform(FieldInfo? vertexField, FieldInfo? fragmentField) 
            {
                VertexField = vertexField;
                FragmentField = fragmentField;
            }
            public override void SetUniform(object value)
            {
                if (value is not UniformType)
                {
                    WindowStatic.Logger.LogWarn($"Uniform Error - Value: {value}  is not correct type: {typeof(UniformType).Name}");
                    return;
                }
                SetField(VertexField, value);
                SetField(FragmentField, value);
            }
            private void SetField(FieldInfo? field, object value)
            {
                if (field == null) return;
                try
                {
                    field.SetValue(null, value);
                    return;
                }
                catch (FieldAccessException) { }
                catch (TargetException) { }
                catch (ArgumentException) { }
                WindowStatic.Logger.LogWarn($"There was an unexpected error with setting of uniform {field.Name} with value {value}");
            }
        }

    }
}
