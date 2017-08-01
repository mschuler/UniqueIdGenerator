using System;

namespace UniqueIdGenerator.NetCore
{
    public static class IdConverter
    {
        /// <summary>
        /// ie AAC3UcJAMAA -> 201561779220480
        /// </summary>
        public static ulong ToLong(string id)
        {
            var bytes = Convert.FromBase64String(id);
            Array.Reverse(bytes);
            return BitConverter.ToUInt64(bytes, 0);
        }

        /// <summary>
        /// ie 201561779220480 -> AAC3UcJAMAA
        /// </summary>
        public static string ToString(ulong id)
        {
            var bytes = BitConverter.GetBytes(id);
            Array.Reverse(bytes);
            return Convert.ToBase64String(bytes);
        }
    }
}