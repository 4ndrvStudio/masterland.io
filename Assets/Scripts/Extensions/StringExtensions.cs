using System;
using UnityEngine;

namespace masterland
{
    public static class StringExtensions
    {
        public static string ConvertEnumToStringWithSpace(this Enum input)
        {
            string enumName = input.ToString();
            string modifiedName = enumName.Replace("_", " ");
            return modifiedName;
        }

        public static string ToShortAddress(this string input)
        {
            if (string.IsNullOrEmpty(input) || input.Length <= 12)
                return input;
            

            string shortAddress = input.Substring(0, 7) + "..." + input.Substring(input.Length - 5);
            return shortAddress;
        }
    }
}
