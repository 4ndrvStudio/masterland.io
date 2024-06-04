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
    }
}
