using CPU_Doom.Buffers;
using CPU_Doom.Interfaces;
using CPU_Doom.Types;
using System.Reflection;
using OpenTK.Mathematics;

namespace CPU_Doom.Shaders
{
    public abstract class ShaderProgram
    {
        public abstract void Draw(FrameBuffer2d frameBuffer, VertexArrayObject vertexArray, FrameBuffer2d? depthBuffer);
        public abstract void SetUniform(string name, object value);
        public abstract int SetTexture1d(TextureBuffer1d texture, int texturePos = -1);
        public abstract int SetTexture2d(TextureBuffer2d texture, int texturePos = -1);
        public abstract TextureBuffer1d? GetTexture1d(int texturePos);
        public abstract TextureBuffer2d? GetTexture2d(int texturePos);
    }

    public class ShaderProgram<TVER, TFRAG> : ShaderProgram where TVER : IVertexShader, new() where TFRAG : IFragmentShader, new()//Connect Vertex and Fragment Shaders.
    {
        Type _vertexType, _fragmentType;
        ShaderLinker _linker;
        ShaderTextureHandler _textureHandler = new ShaderTextureHandler();
        ShaderFunctions _functions;

        const int _FRAGTHREADS = 1024;
        const bool _DEBUGMODE = true;
        TFRAG[] _fragShaders = new TFRAG[_FRAGTHREADS];
        public ShaderProgram()
        {
            _vertexType = typeof(TVER);
            _fragmentType = typeof(TFRAG);
            _linker = new ShaderLinker(_vertexType, _fragmentType);
            _functions = new ShaderFunctions(this);
            _fragShaders = (from _ in Enumerable.Range(0, _FRAGTHREADS) select new TFRAG()).ToArray();
        }
        public override void Draw(FrameBuffer2d frameBuffer, VertexArrayObject vertexArray, FrameBuffer2d? depthBuffer)
        {
            TVER[] vertices = RunVertex(vertexArray);
            RunFragment(vertices, vertexArray, frameBuffer, depthBuffer);
        }

        private TVER[] RunVertex(VertexArrayObject vertexArray)
        {
            int vertexCount = vertexArray.Vertices.Size;
            TVER[] vertices = new TVER[vertexCount];

            for (int i = 0; i < vertexCount; ++i)
            {
                var vertex = vertexArray.Vertices.Get(i);
                TVER ver = vertices[i] = new TVER();
                int j = 0;
                foreach (byte[] vertexAttribute in vertex)
                {
                    FieldInfo field = _linker.VertexInputs[j];
                    field.AssignByteArrayToField(ver, vertexAttribute);
                    j++;
                }
                ver.Execute(_functions);
                ver.Position = ver.Position / ver.Position.W; // Project to 3D space
                ver.Position = new Vector4( ver.Position.X, -ver.Position.Y, ver.Position.Z, 1 );
            }
            return vertices;
        }

        private void RunFragment(TVER[] vertices, VertexArrayObject vertexArray, FrameBuffer2d frameBuffer, FrameBuffer2d? depthBuffer)
        {
            Vector2 frameBufferSize = new Vector2(frameBuffer.RowSize, frameBuffer.Size) / 2; // Divide by two here to save computation when converting Clip space to FrameBuffer space
            for (int i = 0; i < vertexArray.Indices.Length; i += 3)
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
                float minX = MathHelper.Clamp(MathF.Floor(MathVec.Min3(posA.X, posB.X, posC.X)), 0f, frameBuffer.RowSize) + 0.5f;
                float minY = MathHelper.Clamp(MathF.Floor(MathVec.Min3(posA.Y, posB.Y, posC.Y)), 0f, frameBuffer.Size) + 0.5f;
                float maxX = MathHelper.Clamp(MathF.Ceiling(MathVec.Max3(posA.X, posB.X, posC.X)), 0f, frameBuffer.RowSize);
                float maxY = MathHelper.Clamp(MathF.Ceiling(MathVec.Max3(posA.Y, posB.Y, posC.Y)), 0f, frameBuffer.Size);
               
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

                // Calculate internal Fragments of the Bounding Box
                int interX = (int)MathF.Ceiling(maxX - minX);
                int interY = (int)MathF.Ceiling(maxY - minY);
                int inter = interX * interY;

                //Run the Fragment Loop
                if (inter > _FRAGTHREADS && _DEBUGMODE == false)
                {
                    int fragPerThread = inter / _FRAGTHREADS;

                    Parallel.For(0, _FRAGTHREADS, threadOperation =>
                    {
                        int fragPerThisThread = threadOperation == _FRAGTHREADS - 1 ? fragPerThread : inter - (_FRAGTHREADS - 1) * fragPerThread;

                        TFRAG shader = _fragShaders[threadOperation];
                        for (int threadFrag = 0; threadFrag < fragPerThisThread; threadFrag++)
                        {
                            int fragIndex = threadOperation * fragPerThread + threadFrag;

                            int localY = fragIndex / interX;
                            int localX = fragIndex - interX * localY;
                            float globalX = minX + localX;
                            float globalY = minY + localY;

                            SetOneFragment(globalX, globalY, triangleArea, data, A, B, C, frameBuffer, depthBuffer, shader);
                        }
                    });
                }
                else
                {
                    TFRAG shader = _fragShaders[0];
                    for (int I = 0; I < interX * interY; I++)
                    {
                        int iY = I / interX;
                        int iX = I - interX * iY;
                        float x = minX + iX;
                        float y = minY + iY;

                        SetOneFragment(x, y, triangleArea, data, A, B, C, frameBuffer, depthBuffer, shader);
                    }
                }
            }
        }
        int debugCounter = 0;
        private void SetOneFragment(float x, float y, float triangleArea, TriangleData data, TVER A, TVER B, TVER C, FrameBuffer2d frameBuffer, FrameBuffer2d? depthBuffer, TFRAG fragmentShader)
        {
            Vector2 point = new Vector2(x, y);
            CrossProducts products = GetCrossProducts(data, point);
            if (IsInsideTriangle(products))
            {
                float alpha = products.cA / triangleArea;
                float beta = products.cB / triangleArea;
                float gamma = products.cC / triangleArea;
                foreach (var verOutFragOut in _linker.LinkedVariables.Values)
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
                fragmentShader.Execute(_functions);
                object? fragOutput = _linker.FragmentOutput.GetValue(fragmentShader);
                if (fragOutput != null)
                {
                    int intX = (int)x; int intY = (int)y;

                    if (depthBuffer != null)
                    {
                        float posZ = ((alpha * A.Position.Z + beta * B.Position.Z + gamma * C.Position.Z) + 1) / 2;
                        byte[] bufferZBytes = depthBuffer.Get(intY).Get(intX);
                        float bufferZ = bufferZBytes.ToFloat();

                        if (posZ < bufferZ) return;
                        
                        depthBuffer.Get(intY).Set(intX, posZ.ToByteArray());
                    }

                    byte[] fragOutArr = PixelTypeConverter.GetBytesFromStruct(fragOutput);
                    frameBuffer[intY][intX] = fragOutArr;
                }
            }
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

        public override void SetUniform(string name, object value) => _linker.SetUniform(name, value);
        public override int SetTexture1d(TextureBuffer1d texture, int texturePos = -1) => _textureHandler.SetTexture1d(texture, texturePos);
        public override int SetTexture2d(TextureBuffer2d texture, int texturePos = -1) => _textureHandler.SetTexture2d(texture, texturePos);
        public override TextureBuffer1d? GetTexture1d(int texturePos) => _textureHandler.GetTexture1d(texturePos);
        public override TextureBuffer2d? GetTexture2d(int texturePos) => _textureHandler?.GetTexture2d(texturePos);
    }
}
