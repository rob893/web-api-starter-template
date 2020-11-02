using System;

namespace ProjectTemplate.Extensions
{
    public static class UtilityExtensions
    {
        public static int ConvertToInt32FromBase64(this string str)
        {
            try
            {
                return BitConverter.ToInt32(Convert.FromBase64String(str), 0);
            }
            catch
            {
                throw new ArgumentException($"{str} is not a valid base 64 encoded int32.");
            }
        }

        public static string ConvertInt32ToBase64(this int i)
        {
            return Convert.ToBase64String(BitConverter.GetBytes(i));
        }
    }
}