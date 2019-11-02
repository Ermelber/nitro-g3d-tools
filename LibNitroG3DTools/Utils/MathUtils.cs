using System;
using Assimp;
using LibFoundation.Math;

namespace LibNitroG3DTools.Utils
{
    public static class MathUtils
    {
        //Copied from http://www.chrobotics.com/library/understanding-quaternions XD
        public static Vector3 ToEulerAngles(this Quaternion quat)
        {
            var a = quat.W;
            var b = quat.X;
            var c = quat.Y;
            var d = quat.Z;

            var x = Math.Atan((2 * (a * b + c * d)) / (a * a - b * b - c * c + d * d));
            var y = -Math.Asin(2 * (b * d - a * c));
            var z = Math.Atan((2 * (a * d + b * c)) / (a * a + b * b - c * c - d * d));

            return new Vector3((float)(x * (180.0 / Math.PI)), (float)(y * (180.0 / Math.PI)), (float)(z * (180.0 / Math.PI)));
        }
    }
}
