using System;
using Assimp;
using LibFoundation.Math;
using LibNitro.Intermediate;

namespace LibNitroG3DTools.Utils
{
    public static class MathUtils
    {
        
        public static Vector3 ToEulerAngles(this Quaternion q)
        {
            //Copied from http://www.chrobotics.com/library/understanding-quaternions XD
            //var a = quat.W;
            //var b = quat.X;
            //var c = quat.Y;
            //var d = quat.Z;

            //var x = Math.Atan((2 * (a * b + c * d)) / (a * a - b * b - c * c + d * d));
            //var y = -Math.Asin(2 * (b * d - a * c));
            //var z = Math.Atan((2 * (a * d + b * c)) / (a * a + b * b - c * c - d * d));

            //return new Vector3((float)(x * (180.0 / Math.PI)), (float)(y * (180.0 / Math.PI)), (float)(z * (180.0 / Math.PI)));

            //Vector3D vec = Quaternion.Rotate(new Vector3D(0, 0, 1), quat);
            //vec.Normalize();

            //var x = Math.Sin(vec.X)

            //var ret = new VecFx32((float) (x * (180.0 / Math.PI)), (float) (y * (180.0 / Math.PI)),
            //    (float) (z * (180.0 / Math.PI))).ToVector3();

            //return ret;
            
            // roll (x-axis rotation)
            double sinr_cosp = 2 * (q.W * q.X + q.Y * q.Z);
            double cosr_cosp = 1 - 2 * (q.X * q.X + q.Y * q.Y);
            double roll = Math.Atan2(sinr_cosp, cosr_cosp);

            double pitch;
            // pitch (y-axis rotation)
            double sinp = 2 * (q.W * q.Y - q.Z * q.X);
            if (Math.Abs(sinp) >= 1)
                pitch = sinp < 0 ? (-Math.PI / 2) : (Math.PI / 2); // use 90 degrees if out of range
            else
                pitch = Math.Asin(sinp);

            // yaw (z-axis rotation)
            double siny_cosp = 2 * (q.W * q.Z + q.X * q.Y);
            double cosy_cosp = 1 - 2 * (q.Y * q.Y + q.Z * q.Z);
            double yaw = Math.Atan2(siny_cosp, cosy_cosp);

            return new Vector3(
                (float)(roll * (180.0 / Math.PI)),
                (float)(pitch * (180.0 / Math.PI)),
                (float)(yaw * (180.0 / Math.PI)));
        }
    }
}
