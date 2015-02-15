using OpenTK;

namespace WEditor
{
    public static class MathE
    {
        /// <summary>
        /// Honestly at this point I don't even understand it. Just use this to get a unit axis direction relative to the quaternion.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="vec"></param>
        /// <returns></returns>
        public static Vector3 Multiply(this Quaternion value, Vector3 vec)
        {
            Quaternion vectorQuat, inverseQuat, resultQuat;
            Vector3 resultVector;

            vectorQuat = new Quaternion(vec.X, vec.Y, vec.Z, 0f);
            inverseQuat = value.Invert_Custom();
            resultQuat = vectorQuat * inverseQuat;
            resultQuat = value * resultQuat;

            resultVector = new Vector3(resultQuat.X, resultQuat.Y, resultQuat.Z);
            return resultVector;
        }

        public static Quaternion Invert_Custom(this Quaternion value)
        {
            Quaternion newQuat = new Quaternion(value.X, value.Y, value.Z, value.W);
            float length = 1.0f / ((newQuat.X * newQuat.X) + (newQuat.Y * newQuat.Y) + (newQuat.Z * newQuat.Z) + (newQuat.W * newQuat.W));
            newQuat.X *= -length;
            newQuat.Y *= -length;
            newQuat.Z *= -length;
            newQuat.W *= length;

            return newQuat;
        }

        public static int Clamp(int value, int minValue, int maxValue)
        {
            if (value < minValue)
                return minValue;
            if (value > maxValue)
                return maxValue;

            return value;
        }

        public static float Clamp(float value, float minValue, float maxValue)
        {
            if (value < minValue)
                return minValue;
            if (value > maxValue)
                return maxValue;

            return value;
        }
    }
}
