
using CPU_Doom.Buffers;
using CPU_Doom.Interfaces;
using SFML.System;
using System.Reflection;

namespace CPU_Doom.Shaders
{


    public class ShaderProgram //Connect Vertex and Fragment Shaders.
    {
        Type _vertexType, _fragmentType;

        List<FieldInfo> _vertexInputs = new List<FieldInfo>();
        Dictionary<string, ShaderVariable> _linkedVariables = new Dictionary<string, ShaderVariable>(); // Dictionary for vertexOut/fragmentIn.
        FieldInfo _fragmentOutput;
        Dictionary<string, ShaderUniform> _uniforms = new Dictionary<string, ShaderUniform>();

        public ShaderProgram(Type vertexType, Type fragmentType)
        {
            if (!vertexType.IsAssignableTo(typeof(IVertexShader)) || !fragmentType.IsAssignableTo(typeof(IFragmentShader))) 
                throw new ArgumentException("Provided types are not shaders!!! Use IVertexShader and IFragramentShader for your shaders");

            _vertexType = vertexType;
            _fragmentType = fragmentType;
            LinkShaders();
        }

        public void Draw(FrameBuffer frameBuffer, VertexArrayObject vertexArray)
        {
            foreach (var vertex in vertexArray.Vertices) 
            {

            }
        }


        private void LinkShaders() 
        {
            LinkVariables();
            LinkUniforms();
        }

        private void LinkVariables()
        {
            // Get Input and Output Variables
            GetInputOutputFields(_vertexType, out IEnumerable<FieldInfo> vertexInputs, out IEnumerable<FieldInfo> vertexOutputs);
            GetInputOutputFields(_fragmentType, out IEnumerable<FieldInfo> fragmentInputs, out IEnumerable<FieldInfo> fragmentOutputs);

            if (!fragmentOutputs.HasExactlyOneElement()) return; // TODO: throw an exception. + Logger

            // Process Vertex Inputs

            SortedDictionary<int, FieldInfo> specifiedFields = new SortedDictionary<int, FieldInfo>();
            Queue<FieldInfo> unspecifiedFields = new Queue<FieldInfo>();
            foreach (FieldInfo field in vertexInputs)
            {
                var attribute = field.GetCustomAttribute<InputAttribute>();
                if (attribute == null) continue; //This check is pointless, but it needs to be here so C# compiler won't scream at me with warnings

                if(attribute.Location == -1) unspecifiedFields.Enqueue(field);
                else
                {
                    if (specifiedFields.ContainsKey(attribute.Location)) return; // TODO: Exception + Logger
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
                if (!verOut.FieldType.IsAssignableTo(fragIn.FieldType)) continue; // TODO: Logger

                var shaderVar = new ShaderVariable();
                shaderVar.VertexField = verOut;
                shaderVar.FragmentField = fragIn;
                
                if (_linkedVariables.ContainsKey(verOutAt.Name)) continue; // Logger + Consider exception.
                _linkedVariables[verOutAt.Name] = shaderVar;
            }

            // Process Fragment Output
            var fragOut = fragmentOutputs.First(); // I do not need to catch exception since at the start of the method I'm checking if it has only one element
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
                if (linkedUniform == null) continue; // TODO: Logger
                var unAt = uniform.Item1?.GetCustomAttribute<UniformAttribute>();
                if (unAt == null) unAt = uniform.Item2?.GetCustomAttribute<UniformAttribute>();
                if (unAt == null) continue; // TODO: Logger
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
            uniforms = (from field in fields where field.GetCustomAttribute(typeof(UniformAttribute)) != null && field.FieldType.IsValueType  select field).DistinctBy(
                field =>
                {
                    var at = field.GetCustomAttribute<UniformAttribute>();
                    if (at != null) return at.Name;
                    return ""; // Will never get here because of the code above.
                }
            );
        }

        private class ShaderVariable
        {
            public FieldInfo? VertexField { get; set; }
            public FieldInfo? FragmentField { get; set; }
        }


        abstract class ShaderUniform
        {
            public abstract void SetUniform(object value); // TODO: if performance is gonna be fucked, this encapsulation might be the reason :)

            public static ShaderUniform? LinkUniform(FieldInfo? vertexField, FieldInfo? fragmentField)
            {
                Type? uniformType = TryGetType(vertexField, fragmentField);
                if (uniformType == null) return null;

                Type constructedType;
                if (!constructedUniformCache.ContainsKey(uniformType))
                {
                    Type genericuniform = typeof(ShaderUniform<>);
                    constructedType = genericuniform.MakeGenericType(uniformType);
                    constructedUniformCache.Add(uniformType, constructedType);
                }
                else constructedType = constructedUniformCache[uniformType];
                ShaderUniform? uniform = (ShaderUniform?)Activator.CreateInstance(constructedType, true);
                SetFieldsInUniform(vertexField, fragmentField, uniform, constructedType);
                return uniform;
            }
            private static Type? TryGetType(FieldInfo? vertexField, FieldInfo? fragmentField) // TODO: LOGGER!!!!!!!
            {
                Type? vertexType = vertexField?.FieldType ?? null;
                Type? fragmentType = fragmentField?.FieldType ?? null;
                bool vertexNull = vertexType == null;
                bool fragmentNull = fragmentType == null;
                if (vertexNull && fragmentNull) { return null; }
                else if (vertexNull) { return fragmentType; } 
                else if (fragmentNull) { return vertexType; }
                else if (vertexType == fragmentType) { return vertexType; }
                return null;
            } 

            private class ConstructedTypeProperties
            {
                public PropertyInfo VertexFieldProperty { get; init; }
                public PropertyInfo FragmentFieldProperty { get; init; }
                public ConstructedTypeProperties(Type cType)
                {
                    PropertyInfo? vertexFieldProperty = cType.GetProperty("vertexField");
                    PropertyInfo? fragmentFieldProperty = cType.GetProperty("fragmentField");
                    if (vertexFieldProperty == null || fragmentFieldProperty == null)
                    {
                        throw new MissingFieldException("The property for vertexField and fragmentField does not exists in ShaderUniform Class");
                    }
                    VertexFieldProperty = vertexFieldProperty;
                    FragmentFieldProperty = fragmentFieldProperty;
                }

            }

            private static void SetFieldsInUniform(FieldInfo? vertexField, FieldInfo? fragmentField, ShaderUniform? uniform, Type constructedType)
            {
                if (uniform == null) return;
                ConstructedTypeProperties properties;
                if (!constructedTypePropertyCache.ContainsKey(constructedType))
                {
                    properties = new ConstructedTypeProperties(constructedType);
                    constructedTypePropertyCache.Add(constructedType, properties);
                }
                else properties = constructedTypePropertyCache[constructedType];
                properties.VertexFieldProperty.SetValue(uniform, vertexField);
                properties.FragmentFieldProperty.SetValue(uniform, fragmentField);
            }



            static Dictionary<Type, Type> constructedUniformCache = new Dictionary<Type, Type>();
            static Dictionary<Type, ConstructedTypeProperties> constructedTypePropertyCache = new Dictionary<Type, ConstructedTypeProperties>();

        }

        private class ShaderUniform<UniformType> : ShaderUniform where UniformType : struct
        {
            public FieldInfo? vertexField { get; private set; }
            public FieldInfo? fragmentField { get; private set; }

            private ShaderUniform(){}
            public override void SetUniform(object value)
            {
                if (value is not UniformType) return; // TODO: LOGGER!!!!!!!
                SetField(vertexField, value);
                SetField(fragmentField, value);
            }

            private void SetField(FieldInfo? field, object value)
            {
                if (field == null) return;
                else if (!field.IsStatic) return; // TODO: LOGGER!!!!!!!
                try
                {
                    field.SetValue(null, value);
                }
                catch (FieldAccessException) { }
                catch (TargetException) { }
                catch (ArgumentException) { } // TODO: Logger
            }
        }

    }
}
