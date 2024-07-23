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
        public static Vector2 MinMax3(float A, float B, float C)
        {
            float tmp;
            if (A < B)
            {
                tmp = A; A = B; B = tmp;
            }
            if (A < C) 
            {
                tmp = A; A = C; C = tmp;
            }
            if (B < C)
            {
                tmp = B; B = C; C = tmp;
            }
            return new Vector2 (A, C);
        }

    }
}
