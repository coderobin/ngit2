using System;

namespace NGit.Util.IO
{
    /// <summary>
    /// Temp class
    /// </summary>
    internal class RawText
    {
        private static int FIRST_FEW_BYTES = 8000;

        internal static bool IsBinary(byte[] raw, int length)
        {
            // Same heuristic as C Git
            if (length > FIRST_FEW_BYTES)
            {
                length = FIRST_FEW_BYTES;
            }
            for (int ptr = 0; ptr < length; ptr++)
            {
                if (raw[ptr] == '\0')
                {
                    return true;
                }
            }
            return false;
        }
    }
}