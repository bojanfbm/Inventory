using System.Collections;
using System.Globalization;

namespace BusinessLogic.Utility;

internal static class SgtinHelpers
{
    public static long ToLong(this BitArray bitArray)
    {
        if (bitArray.Length > sizeof(long) * 8)
            throw new ArgumentException($"Argument length must be at most {sizeof(long) * 8} bits.");

        var array = new byte[8];
        bitArray.CopyTo(array, 0);
        return BitConverter.ToInt64(array, 0);
    }

    public static BitArray Reverse(this BitArray bitArray)
    {
        if (!BitConverter.IsLittleEndian)
            return bitArray;

        var length = bitArray.Length;
        var mid = length / 2;

        for (var i = 0; i < mid; i++)
        {
            (bitArray[i], bitArray[length - i - 1]) = (bitArray[length - i - 1], bitArray[i]);
        }

        return bitArray;
    }

    /// <summary>
    /// Check if string contains only hexadecimal digits
    /// </summary>
    /// <param name="chars"></param>
    /// <returns></returns>
    public static bool IsHex(IEnumerable<char> chars)
    {
        //fastest way according to https://stackoverflow.com/a/223854/1081079
        foreach (var c in chars)
        {
            var isHex = (c is >= '0' and <= '9' ||
                         c is >= 'a' and <= 'f' ||
                         c is >= 'A' and <= 'F');

            if (!isHex)
                return false;
        }
        return true;

        //Resharper equivalent but hard to read
        //return chars.Select(c => c is >= '0' and <= '9' or >= 'a' and <= 'f' or >= 'A' and <= 'F').All(isHex => isHex);
    }

    /// <summary>
    /// Convert string of hexadecimal digits into BitArray
    /// </summary>
    /// <param name="hexData"></param>
    /// <returns></returns>
    public static BitArray HexStringToBitArray(string hexData)
    {
        var array = new BitArray(4 * hexData.Length);
        for (var i = 0; i < hexData.Length; i++)
        {
            var b = byte.Parse(hexData[i].ToString(), NumberStyles.HexNumber);
            for (var j = 0; j < 4; j++)
                array.Set(i * 4 + j, (b & 1 << 3 - j) != 0);
        }
        return array;
    }
}