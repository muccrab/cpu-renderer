
using CPU_Doom.Buffers;
using CPU_Doom.Interfaces;
using CPU_Doom.Types;
using SFML.System;
using System.Reflection;
using System.Runtime.InteropServices;
using OpenTK.Mathematics;
using System.Data;
using System.Runtime.Serialization;

namespace CPU_Doom.Shaders
{

    public abstract class ShaderProgram
    {
        public abstract void Draw(FrameBuffer2d frameBuffer, VertexArrayObject vertexArray, FrameBuffer2d? depthBuffer);
        public abstract void SetUniform(string name, object value);
    }


    public class ShaderProgram<TVER, TFRAG> : ShaderProgram where TVER : IVertexShader, new() where TFRAG : IFragmentShader, new()//Connect Vertex and Fragment Shaders.
    {
        Type _vertexType, _fragmentType;

        List<FieldInfo> _vertexInputs = new List<FieldInfo>();
        Dictionary<string, ShaderVariable> _linkedVariables = new Dictionary<string, ShaderVariable>(); // Dictionary for vertexOut/fragmentIn.
        FieldInfo _fragmentOutput;
        Dictionary<string, ShaderUniformPar> _uniforms = new Dictionary<string, ShaderUniformPar>();

        public ShaderProgram()
        {
            _vertexType = typeof(TVER);
            _fragmentType = typeof(TFRAG);
            LinkShaders();
            if (_fragmentOutput == null) throw new ArgumentNullException("Fragment Shader must contain a output."); // Compiller was shouting at me if I did this check elsewhere, so I have it in the constructor.
        }

        public override void Draw(FrameBuffer2d frameBuffer, VertexArrayObject vertexArray, FrameBuffer2d? depthBuffer)
        {
            // Vertex Shader Exxecution
            int vertexCount = vertexArray.Vertices.Size;
            TVER[] vertices = new TVER[vertexCount];

            for (int i = 0; i < vertexCount; ++i) 
            {
                var vertex = vertexArray.Vertices[i];
                TVER ver = vertices[i] = new TVER();
                int j = 0;
                foreach (byte[] vertexAttribute in vertex)
                {
                    FieldInfo field = _vertexInputs[j];
                    field.AssignByteArrayToField(ver, vertexAttribute);
                    j++;
                }
                ver.Execute();
                ver.Position = ver.Position / ver.Position.W; // Project to 3D space
            }
            // Fragment Interpolation
            Vector2 frameBufferSize = new Vector2(frameBuffer.RowSize, frameBuffer.Size) / 2; // Divide by two here to save computation when converting Clip space to FrameBuffer space
            for (int i = 0; i < vertexArray.Indices.Length; i+=3) 
            {
                TVER A = vertices[vertexArray.Indices[i]];
                TVER B = vertices[vertexArray.Indices[i + 1]];
                TVER C = vertices[vertexArray.Indices[i + 2]];

                Vector2 posA, posB, posC, ab, bc, ca;

                void SetUpFrag()
                {
                    posA = (A.Position.Xy + Vector2.One) * frameBufferSize;
                    posB = (B.Position.Xy + Vector2.One) * frameBufferSize;
                    posC = (C.Position.Xy + Vector2.One) * frameBufferSize;

                    ab = posB - posA;
                    bc = posC - posB;
                    ca = posA - posC;
                }
                SetUpFrag();
                if (MathVec.Vec2Cross(ab, bc) > 0)
                {
                    TVER tmp = B;
                    B = C;
                    C = tmp;
                    SetUpFrag();
                }

                float triangleArea = MathVec.Vec2Cross(ab, -ca);

                // Calculate Bounding Box of a Triangle.
                float minX =  MathHelper.Clamp(MathF.Floor(MathVec.Min3(posA.X, posB.X, posC.X)), 0f, frameBuffer.RowSize) + 0.5f;
                float minY =  MathHelper.Clamp(MathF.Floor(MathVec.Min3(posA.Y, posB.Y, posC.Y)), 0f, frameBuffer.Size) + 0.5f;
                float maxX =  MathHelper.Clamp(MathF.Ceiling(MathVec.Max3(posA.X, posB.X, posC.X)), 0f, frameBuffer.RowSize);
                float maxY =  MathHelper.Clamp(MathF.Ceiling(MathVec.Max3(posA.Y, posB.Y, posC.Y)), 0f, frameBuffer.Size);

                // Create reference to data.
                TriangleData data = new TriangleData()
                {
                    posA = posA,
                    posB = posB,
                    posC = posC,
                    AB = ab,
                    BC = bc,
                    CA = ca,
                    backwards = false
                };

                int interX = (int)MathF.Ceiling(maxX - minX);
                int interY = (int)MathF.Ceiling(maxY - minY);
                /*
                ThreadManager manager = new ThreadManagerFor(0, interX * interY, i => {
                    int iY = i / interX;
                    int iX = i - interX * iY;

                    float x = minX + iX;
                    float y = minY + iY;

                    Vector2 point = new Vector2(x, y);
                    if (IsInsideTriangle(data, point))
                        frameBuffer[(int)y][(int)x] = (from _ in Enumerable.Range(0, frameBuffer.TypeLength) select (byte)255).ToArray();
                });
                manager.Execute();
                while (manager.Finished) { }
                */
                
                Parallel.For(0, interX * interY, i =>
                {
                    int iY = i / interX;
                    int iX = i - interX * iY;

                    float x = minX + iX;
                    float y = minY + iY;

                    Vector2 point = new Vector2(x, y);
                    CrossProducts products = GetCrossProducts(data, point);
                    if (IsInsideTriangle(products))
                    {
                        TFRAG fragmentShader = new TFRAG();
                        float alpha = products.cA / triangleArea;
                        float beta = products.cB / triangleArea;
                        float gamma = products.cC / triangleArea;
                        foreach (var verOutFragOut in _linkedVariables.Values)
                        {
                            if (verOutFragOut == null) continue;

                            object? valueA = verOutFragOut.VertexField.GetValue(A);
                            object? valueB = verOutFragOut.VertexField.GetValue(B);
                            object? valueC = verOutFragOut.VertexField.GetValue(C);
                            if (valueA == null || valueB == null || valueC == null) continue;

                            bool filtering = verOutFragOut.FileringEnabled;

                            object fragValue = FilterTriangle(alpha, beta, gamma, valueA, valueB, valueC, ref filtering);

                            verOutFragOut.FileringEnabled = filtering;

                            object realFragValue = Convert.ChangeType(fragValue, verOutFragOut.FragmentField.FieldType);
                            verOutFragOut.FragmentField.SetValue(fragmentShader, realFragValue);
                        }
                        fragmentShader.Execute();
                        object? fragOutput = _fragmentOutput.GetValue(fragmentShader);
                        if (fragOutput != null)
                        {
                            int intX = (int)x; int intY = (int)y;

                            if (depthBuffer != null)
                            {
                                float posZ = ((alpha * A.Position.Z + beta * B.Position.Z + gamma * C.Position.Z) + 1) / 2;
                                byte[] bufferZBytes = depthBuffer.Get(intY).Get(intX);
                                float bufferZ = bufferZBytes.ToFloat();

                                if (posZ - bufferZ < 0)
                                {
                                    return;
                                }
                                depthBuffer.Get(intY).Set(intX, posZ.ToByteArray());
                            }

                            byte[] fragOutArr = PixelTypeConverter.GetBytesFromStruct(fragOutput);
                            frameBuffer[intY][intX] = fragOutArr;
                        }
                    }

                });
                
                /*
                for (int I = 0; I < interX * interY; I++)
                {
                    int iY = I / interX;
                    int iX = I - interX * iY;

                    float x = minX + iX;
                    float y = minY + iY;

                    Vector2 point = new Vector2(x, y);
                    CrossProducts products = GetCrossProducts(data, point);
                    if (IsInsideTriangle(products))
                    {
                        TFRAG fragmentShader = new TFRAG();
                        float alpha = products.cA / triangleArea;
                        float beta = products.cB / triangleArea;
                        float gamma = products.cC / triangleArea;
                        foreach (var verOutFragOut in _linkedVariables.Values)
                        {
                            if (verOutFragOut == null) continue;

                            object? valueA = verOutFragOut.VertexField.GetValue(A);
                            object? valueB = verOutFragOut.VertexField.GetValue(B);
                            object? valueC = verOutFragOut.VertexField.GetValue(C);
                            if (valueA == null || valueB == null || valueC == null) continue;

                            bool filtering = verOutFragOut.FileringEnabled;

                            object fragValue = FilterTriangle(alpha, beta, gamma, valueA, valueB, valueC, ref filtering);

                            verOutFragOut.FileringEnabled = filtering;

                            object realFragValue = Convert.ChangeType(fragValue, verOutFragOut.FragmentField.FieldType);
                            verOutFragOut.FragmentField.SetValue(fragmentShader, realFragValue);
                        }
                        fragmentShader.Execute();
                        object? fragOutput = _fragmentOutput.GetValue(fragmentShader);
                        if (fragOutput != null)
                        {
                            int intX = (int)x; int intY = (int)y;

                            if (depthBuffer != null)
                            {
                                float posZ = ((alpha * A.Position.Z + beta * B.Position.Z + gamma * C.Position.Z) + 1) / 2;
                                byte[] bufferZBytes = depthBuffer.Get(intY).Get(intX);
                                float bufferZ = bufferZBytes.ToFloat();

                                if (posZ - bufferZ < 0)
                                {
                                    continue;
                                }
                                depthBuffer.Get(intY).Set(intX, posZ.ToByteArray());
                            }

                            byte[] fragOutArr = PixelTypeConverter.GetBytesFromStruct(fragOutput);
                            frameBuffer[intY][intX] = fragOutArr;
                        }
                    }
                }
                */

                /*
                for (float x = minX; x < maxX; x++)
                {
                    for (float y = minY; y < maxY; y++) 
                    {
                        Vector2 point = new Vector2(x, y);
                        if (IsInsideTriangle(data, point))
                            frameBuffer[(int)y][(int)x] = (from _ in Enumerable.Range(0, frameBuffer.TypeLength) select (byte)255).ToArray();
                    }
                }*/

            }
            // Fragment Shader Interpolation


            // FrameBuffer write
        }

        private class TriangleData
        {
            public Vector2 posA, posB, posC;
            public Vector2 AB, BC, CA;
            public bool backwards = false;
        }

        private struct CrossProducts
        {
            public float cA, cB, cC;
        }

        private CrossProducts GetCrossProducts(TriangleData data, Vector2 point)
        {
            Vector2 pA = data.posA - point;
            Vector2 pB = data.posB - point;
            Vector2 pC = data.posC - point;

            return new CrossProducts
            {
                    cC = MathVec.Vec2Cross(pA, data.AB),
                    cA = MathVec.Vec2Cross(pB, data.BC),
                    cB = MathVec.Vec2Cross(pC, data.CA) 
            };
        }

        private bool IsInsideTriangle(CrossProducts products) => products.cA < 0 && products.cB < 0 && products.cC < 0;

        private object FilterTriangle(float coefA, float coefB, float coefC, dynamic a, dynamic b, dynamic c, ref bool supportsFilter) 
        {
            if (supportsFilter)
            {
                try
                {
                    return coefA * a + coefB * b + coefC * c;
                }
                catch {
                    supportsFilter = false;
                }
            }
            if (coefA > MathF.Max(coefB, coefC)) return a;
            else if (coefB > coefC) return b;
            else return c;
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

                var shaderVar = new ShaderVariable(verOut, fragIn, SupportsFiltering(verOut.FieldType)); // TODO: Better Filtering check. Add check that 
                
                if (_linkedVariables.ContainsKey(verOutAt.Name)) continue; // Logger + Consider exception.
                _linkedVariables[verOutAt.Name] = shaderVar;
            }

            // Process Fragment Output
            if (!fragmentOutputs.HasExactlyOneElement()) return; // TODO: throw an exception. + Logger
            var fragOut = fragmentOutputs.First();
            if (!fragOut.FieldType.GetCustomAttributes(typeof(SerializableAttribute), true).Any()) return; // TODO: throw an exception. + Logger
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
                ShaderUniformPar? linkedUniform = ShaderUniformPar.LinkUniform(uniform.Item1, uniform.Item2);
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

        private static bool SupportsFiltering(Type type) =>
            type.GetMethod("op_Multiply", BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(float), type }, null)?.ReturnType == type &&
            type.GetMethod("op_Addition", BindingFlags.Static | BindingFlags.Public, null, new[] { type, type }, null)?.ReturnType == type;

        public override void SetUniform(string name, object value)
        {
            if (_uniforms.ContainsKey(name))
            _uniforms[name].SetUniform(value); //TODO: Logger
        }

        private class ShaderVariable
        {
            public ShaderVariable(FieldInfo vertexField, FieldInfo fragmentField, bool fileringEnabled)
            {
                VertexField = vertexField;
                FragmentField = fragmentField;
                FileringEnabled = fileringEnabled;
            }

            public FieldInfo VertexField { get; private init; }
            public FieldInfo FragmentField { get; private init; }
            public bool FileringEnabled { get; set; }
        }


        abstract class ShaderUniformPar
        {
            public abstract void SetUniform(object value); 

            public static ShaderUniformPar? LinkUniform(FieldInfo? vertexField, FieldInfo? fragmentField)
            {
                Type? uniformType = TryGetType(vertexField, fragmentField);
                if (uniformType == null) return null;

                Type constructedType;
                if (!constructedUniformCache.ContainsKey(uniformType))
                {
                    Type genericuniform = typeof(ShaderUniform<>);
                    constructedType = genericuniform.MakeGenericType(typeof(TVER), typeof(TFRAG), uniformType);
                    constructedUniformCache.Add(uniformType, constructedType);
                }
                else constructedType = constructedUniformCache[uniformType];
                ShaderUniformPar? uniform = (ShaderUniformPar?)Activator.CreateInstance(constructedType, true);
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

            private static void SetFieldsInUniform(FieldInfo? vertexField, FieldInfo? fragmentField, ShaderUniformPar? uniform, Type constructedType)
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

        private class ShaderUniform<UniformType> : ShaderUniformPar where UniformType : struct
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
