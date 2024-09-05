using CPU_Doom.Shaders;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo
{
    internal class Transform3D : ObjectComponent
    {
        public Vector3 Position;
        public Vector3 Rotation;
        public Vector3 Scale = Vector3.One;

        private Vector3 GetFullPosition()
        {
            Vector3 realPos = ParentObject?.Parent?.Transform.GetFullPosition() ?? Vector3.Zero;
            return realPos + Position; 
        }

        private Vector3 GetFullScale()
        {
            if (Scale == Vector3.Zero) return Vector3.Zero;
            Vector3 realScale = ParentObject?.Parent?.Transform.GetFullScale() ?? Vector3.One;
            return realScale * Scale;
        }

        private Vector3 GetFullRotation()
        {
            Vector3 realRotation = ParentObject?.Parent?.Transform.GetFullRotation() ?? Vector3.Zero;
            return realRotation + Rotation;
        }

        public Matrix4 GetModelMatrix()
        {
            Vector3 realPos = GetFullPosition();
            Vector3 realScale = GetFullScale();
            Vector3 realRot = GetFullRotation();

            Matrix4 model = Matrix4.CreateScale(realScale) * Matrix4.CreateRotationX(realRot.X) * Matrix4.CreateRotationY(realRot.Y) * Matrix4.CreateRotationZ(realRot.Z);
            model.M14 = realPos.X; model.M24 = realPos.Y; model.M34 = realPos.Z;
            return model;
        }

        public void SetShader(ShaderProgram shader)
        {
            var model = GetModelMatrix();
            shader.SetUniform("u_Model", model);
        }
    }
}
