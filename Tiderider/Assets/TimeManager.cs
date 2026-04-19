using System;
using System.Globalization;

public static class TimeManager
{
    /// <summary>
    /// Gets the current UTC time formatted as an ISO 8601 string to prevent locale parsing errors.
    /// </summary>
    public static string GetCurrentTimeString()
    {
        return DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Gets an ISO 8601 string offset by a specific number of seconds. Useful for offline recovery math.
    /// </summary>
    public static string GetAdjustedTimeString(double secondsOffset)
    {
        return DateTime.UtcNow.AddSeconds(secondsOffset).ToString("O", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Calculates how much time has passed since a saved UTC time string.
    /// </summary>
    public static TimeSpan GetTimePassed(string savedTimeString)
    {
        if (string.IsNullOrEmpty(savedTimeString))
            return TimeSpan.MaxValue; // If no time was saved, assume a very long time has passed

        // RoundtripKind ensures it respects the UTC formatting from the "O" string
        if (DateTime.TryParse(savedTimeString, null, DateTimeStyles.RoundtripKind, out DateTime savedTime))
        {
            return DateTime.UtcNow - savedTime.ToUniversalTime();
        }

        return TimeSpan.Zero;
    }

    /// <summary>
    /// Checks if a specific duration has passed since the saved time.
    /// </summary>
    public static bool HasPassed(string savedTimeString, TimeSpan duration)
    {
        return GetTimePassed(savedTimeString) >= duration;
    }

    /// <summary>
    /// Returns a formatted string (MM:SS or HH:MM:SS) for remaining cooldown time.
    /// </summary>
    public static string GetRemainingTimeFormatted(string savedTimeString, TimeSpan duration)
    {
        TimeSpan passed = GetTimePassed(savedTimeString);
        if (passed >= duration) return "Ready";

        TimeSpan remaining = duration - passed;
        if (remaining.TotalHours >= 1)
            return string.Format("{0:D2}:{1:D2}:{2:D2}", remaining.Hours, remaining.Minutes, remaining.Seconds);

        return string.Format("{0:D2}:{1:D2}", remaining.Minutes, remaining.Seconds);
    }
}