using CPU_Doom.Buffers;
using CPU_Doom.Interfaces;
using CPU_Doom.Types;
using System.Reflection;
using OpenTK.Mathematics;
using SFML.Graphics;

namespace CPU_Doom.Shaders
{
    /// <summary>
    /// Abstract base class for shader programs.
    /// </summary>
    public abstract class ShaderProgram
    {
        /// <summary>
        /// Renders a vertex array object to a frame buffer using this shader program.
        /// </summary>
        /// <param name="frameBuffer">The frame buffer to which the vertex array is rendered.</param>
        /// <param name="vertexArray">The vertex array object containing vertices.</param>
        /// <param name="depthBuffer">Optional depth buffer for depth testing.</param>
        public abstract void Draw(FrameBuffer2d frameBuffer, VertexArrayObject vertexArray, FrameBuffer2d? depthBuffer);

        /// <summary>
        /// Sets a uniform variable in the shader program.
        /// </summary>
        /// <param name="name">The name of the uniform variable.</param>
        /// <param name="value">The value to set for the uniform variable.</param>
        public abstract void SetUniform(string name, object value);

        /// <summary>
        /// Binds a 1D texture to the shader program at the specified texture position.
        /// </summary>
        /// <param name="texture">The 1D texture to bind.</param>
        /// <param name="texturePos">The texture position to bind to. Default is -1.</param>
        /// <returns>The texture position used.</returns>
        public abstract int SetTexture1d(TextureBuffer1d texture, int texturePos = -1);

        /// <summary>
        /// Binds a 2D texture to the shader program at the specified texture position.
        /// </summary>
        /// <param name="texture">The 2D texture to bind.</param>
        /// <param name="texturePos">The texture position to bind to. Default is -1.</param>
        /// <returns>The texture position used.</returns>
        public abstract int SetTexture2d(TextureBuffer2d texture, int texturePos = -1);

        /// <summary>
        /// Retrieves a 1D texture from the shader program at the specified texture position.
        /// </summary>
        /// <param name="texturePos">The texture position to retrieve from.</param>
        /// <returns>The 1D texture at the specified position, or null if not found.</returns>
        public abstract TextureBuffer1d? GetTexture1d(int texturePos);

        /// <summary>
        /// Retrieves a 2D texture from the shader program at the specified texture position.
        /// </summary>
        /// <param name="texturePos">The texture position to retrieve from.</param>
        /// <returns>The 2D texture at the specified position, or null if not found.</returns>
        public abstract TextureBuffer2d? GetTexture2d(int texturePos);
    }

    /// <summary>
    /// Generic shader program implementation using specified vertex and fragment shader types.
    /// </summary>
    /// <typeparam name="TVER">The type of the vertex shader, must implement <see cref="IVertexShader"/> and have a parameterless constructor.</typeparam>
    /// <typeparam name="TFRAG">The type of the fragment shader, must implement <see cref="IFragmentShader"/> and have a parameterless constructor.</typeparam>
    public class ShaderProgram<TVER, TFRAG> : ShaderProgram where TVER : IVertexShader, new() where TFRAG : IFragmentShader, new()
    {
        private Type _vertexType, _fragmentType; // types of vertex and fragment shaders
        private ShaderLinker _linker; 
        private ShaderTextureHandler _textureHandler = new ShaderTextureHandler(); 
        private ShaderFunctions _functions; 

        const int _FRAGTHREADS = 1024; // Number of threads fragment shader will work on
        const bool _DEBUGMODE = true; // Debug Constant to run Fragment Shader in one thread
        TFRAG[] _fragShaders = new TFRAG[_FRAGTHREADS]; // array for fragment shader for one specific thread.

        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderProgram{TVER, TFRAG}"/> class.
        /// </summary>
        public ShaderProgram()
        {
            _vertexType = typeof(TVER);
            _fragmentType = typeof(TFRAG);
            _linker = new ShaderLinker(_vertexType, _fragmentType);
            _functions = new ShaderFunctions(this);
            _fragShaders = (from _ in Enumerable.Range(0, _FRAGTHREADS) select new TFRAG()).ToArray();
        }

        /// <summary>
        /// Renders a vertex array object to a frame buffer using this shader program.
        /// </summary>
        /// <param name="frameBuffer">The frame buffer to which the vertex array is rendered.</param>
        /// <param name="vertexArray">The vertex array object containing vertices.</param>
        /// <param name="depthBuffer">Optional depth buffer for depth testing.</param>
        public override void Draw(FrameBuffer2d frameBuffer, VertexArrayObject vertexArray, FrameBuffer2d? depthBuffer) 
        {
            TVER[] vertices = RunVertex(vertexArray);
            RunFragment(vertices, vertexArray, frameBuffer, depthBuffer);
        }

        private TVER[] RunVertex(VertexArrayObject vertexArray)
        {
            int vertexCount = vertexArray.Vertices.Size;
            TVER[] vertices = new TVER[vertexCount];

            for(int i = 0; i < vertexCount; ++i)
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
                if (MathVec.Vec2Cross(ab, bc) > 0) // If vertices are in wrong order, switch and setup again
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

                            RunOneFragment(shader, globalX, globalY, triangleArea, data, A, B, C, frameBuffer, depthBuffer);
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

                        RunOneFragment(shader, x, y, triangleArea, data, A, B, C, frameBuffer, depthBuffer);
                    }
                }
            }
        }
        
        // Sets up and runs one fragment shader
        private void RunOneFragment(TFRAG fragmentShader, float x, float y, float triangleArea, TriangleData data, TVER A, TVER B, TVER C, FrameBuffer2d frameBuffer, FrameBuffer2d? depthBuffer)
        {
            Vector2 point = new Vector2(x, y);
            CrossProducts products = GetCrossProducts(data, point);
            if (IsInsideTriangle(products))
            {
                // calculates areas for parts of the tringle
                float alpha = products.cA / triangleArea;
                float beta = products.cB / triangleArea;
                float gamma = products.cC / triangleArea;
                // Interpolates vertex Outputs to Fragment Inputs
                foreach (var verOutFragIn in _linker.LinkedVariables.Values)
                {
                    if (verOutFragIn == null) continue;

                    object? valueA = verOutFragIn.VertexField.GetValue(A);
                    object? valueB = verOutFragIn.VertexField.GetValue(B);
                    object? valueC = verOutFragIn.VertexField.GetValue(C);
                    if (valueA == null || valueB == null || valueC == null) continue;

                    bool filtering = verOutFragIn.FileringEnabled;

                    object fragValue = FilterTriangle(alpha, beta, gamma, valueA, valueB, valueC, ref filtering);

                    verOutFragIn.FileringEnabled = filtering;

                    object realFragValue = Convert.ChangeType(fragValue, verOutFragIn.FragmentField.FieldType);
                    verOutFragIn.FragmentField.SetValue(fragmentShader, realFragValue);
                }
                // Execute Fragment Shader
                fragmentShader.Execute(_functions);
                // Draw fragment output to buffer. Apply Depth Filtering
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
                cC = MathVec.Vec2Cross(pA, data.AB), // Area that influents value from vertex C
                cA = MathVec.Vec2Cross(pB, data.BC), // Area that influents value from vertex A
                cB = MathVec.Vec2Cross(pC, data.CA)  // Area that influents value from vertex B  
            };
        }
        private bool IsInsideTriangle(CrossProducts products) => products.cA < 0 && products.cB < 0 && products.cC < 0; 

        // Tries to apply filtering, if it can't, dissables filter for next itteration and returns value of the vertex with maximum influence
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

        /// <summary>
        /// Sets a uniform variable in the shader program.
        /// </summary>
        /// <param name="name">The name of the uniform variable.</param>
        /// <param name="value">The value to set for the uniform variable.</param>
        public override void SetUniform(string name, object value) => _linker.SetUniform(name, value);

        /// <summary>
        /// Binds a 1D texture to the shader program at the specified texture position.
        /// </summary>
        /// <param name="texture">The 1D texture to bind.</param>
        /// <param name="texturePos">The texture position to bind to. Default is -1.</param>
        /// <returns>The texture position used.</returns>
        public override int SetTexture1d(TextureBuffer1d texture, int texturePos = -1) => _textureHandler.SetTexture1d(texture, texturePos);

        /// <summary>
        /// Binds a 2D texture to the shader program at the specified texture position.
        /// </summary>
        /// <param name="texture">The 2D texture to bind.</param>
        /// <param name="texturePos">The texture position to bind to. Default is -1.</param>
        /// <returns>The texture position used.</returns>
        public override int SetTexture2d(TextureBuffer2d texture, int texturePos = -1) => _textureHandler.SetTexture2d(texture, texturePos);

        /// <summary>
        /// Retrieves a 1D texture from the shader program at the specified texture position.
        /// </summary>
        /// <param name="texturePos">The texture position to retrieve from.</param>
        /// <returns>The 1D texture at the specified position, or null if not found.</returns>
        public override TextureBuffer1d? GetTexture1d(int texturePos) => _textureHandler.GetTexture1d(texturePos);

        /// <summary>
        /// Retrieves a 2D texture from the shader program at the specified texture position.
        /// </summary>
        /// <param name="texturePos">The texture position to retrieve from.</param>
        /// <returns>The 2D texture at the specified position, or null if not found.</returns>
        public override TextureBuffer2d? GetTexture2d(int texturePos) => _textureHandler?.GetTexture2d(texturePos);
    }
}
