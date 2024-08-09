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

        public static string ToSuiCoinFormat(this string amount)
        {
            decimal balanceValue = decimal.Parse(amount) / 1000000000;
            return balanceValue.ToString("0.#########");
        }

        public static string ConvertTimestampToMinutesAndSeconds(this string timestamp)
        {
            long timestampSeconds = long.Parse(timestamp);
            // Create a DateTimeOffset from Unix epoch
            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(timestampSeconds);

            // Calculate the total number of minutes from the Unix epoch
            TimeSpan timeSpanSinceEpoch = dateTimeOffset - DateTimeOffset.FromUnixTimeSeconds(0);
            int totalMinutes = (int)timeSpanSinceEpoch.TotalMinutes;

            // Calculate hours and minutes from total minutes
            int hours = totalMinutes / 60;
            int minutes = totalMinutes % 60;

            // Format the result as "HH:MM"
            string formattedTime = string.Format("{0:D2}:{1:D2}", hours, minutes);
            Debug.Log(formattedTime);
            return formattedTime;
        }
    }
}
