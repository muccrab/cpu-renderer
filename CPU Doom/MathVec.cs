using OpenTK.Mathematics;

namespace CPU_Doom
{
    internal static class MathVec
    {
        public static float Vec2Cross(Vector2 v1,  Vector2 v2) => v1.X * v2.Y - v1.Y * v2.X;

        public static float Min3(float A, float B, float C) => (A < B) ?
                                                               (A < C ? A : C):
                                                               (B < C ? B : C);

        public static float Max3(float A, float B, float C) => (A > B) ?
                                                               (A > C ? A : C):
                                                               (B > C ? B : C);

    }
}
