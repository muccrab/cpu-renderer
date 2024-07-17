
using CPU_Doom.Buffers;
using CPU_Doom.Interfaces;
using SFML.System;
using System.Reflection;

namespace CPU_Doom.Shaders
{


    public class ShaderProgram //Connect Vertex and Fragment Shaders.
    {
        Type _vertexType, _fragmentType;


        Dictionary<string, ShaderVariable> _variables = new Dictionary<string, ShaderVariable>();
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

        }


        private void LinkShaders() 
        {
            #region Variables
            GetInputOutputFields(_vertexType, out IEnumerable<FieldInfo> vertexInputs, out IEnumerable<FieldInfo> vertexOutputs);
            GetInputOutputFields(_fragmentType, out IEnumerable<FieldInfo> fragmentInputs, out IEnumerable<FieldInfo> fragmentOutputs);

            // TODO: check fragments outputs....only one
            if (!fragmentOutputs.HasExactlyOneElement()) return; // TODO: throw an exception. + Logger



            foreach (FieldInfo field in vertexInputs) 
            {
                var attribute = field.GetCustomAttribute<InputAttribute>();
                if (attribute == null) continue; //This check is pointless, but it needs to be here so C# compiler won't scream at me with warnings
                var shaderVar = new ShaderVariable();
                shaderVar.vertexField = field;
                shaderVar.vertexLocation = attribute.Location; // TODO: Automatize Locations!!!!!
                shaderVar.vertexType = SHADERVARTYPE.IN;
                _variables.Add(attribute.Name, shaderVar);
            }

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
                if (verOutAt ==  null || fragInAt == null) continue; //Again pointless, but I won't get warnings
                
                if (!verOut.FieldType.IsAssignableTo(fragIn.FieldType)) continue; // TODO: Logger
                var shaderVar = new ShaderVariable();
                shaderVar.vertexField = verOut;
                shaderVar.vertexType = SHADERVARTYPE.OUT;
                shaderVar.fragmentField = fragIn;
                shaderVar.fragmentType = SHADERVARTYPE.IN;
                if (_variables.ContainsKey(verOutAt.Name)) continue; // Logger + Consider exception.
                _variables[verOutAt.Name] = shaderVar;
            }

            var fragOut = fragmentOutputs.First(); // I do not need to catch exception since at the start of the method I'm checking if it has only one element
            var fragOutAt = fragOut.GetCustomAttribute<OutputAttribute>();
            if (fragOutAt == null) return; // TODO: throw an exception. + Logger
            if (_variables.ContainsKey(fragOutAt.Name))
            {
                if (_variables[fragOutAt.Name].fragmentField == null)
                {
                    _variables[fragOutAt.Name].fragmentField = fragOut;
                    _variables[fragOutAt.Name].fragmentType = SHADERVARTYPE.OUT;
                }
                else return; // TODO: throw an exception. + Logger
            }
            else
            {
                var shaderVar = new ShaderVariable();
                shaderVar.fragmentField = fragOut;
                shaderVar.fragmentType = SHADERVARTYPE.OUT;
                _variables.Add(fragOutAt.Name, shaderVar);
            }
            #endregion
            #region Uniforms

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
                if (unAt == null)   unAt = uniform.Item2?.GetCustomAttribute<UniformAttribute>();
                if (unAt == null)    continue; // TODO: Logger
                _uniforms.Add(unAt.Name, linkedUniform);
            }
            #endregion
            // TODO: finish this method
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
            public SHADERVARTYPE vertexType;
            public SHADERVARTYPE fragmentType;
            public int vertexLocation = -1;
            public FieldInfo? vertexField;
            public FieldInfo? fragmentField;
        }

        private enum SHADERVARTYPE
        {
            IN, OUT, UNDEFINED
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
