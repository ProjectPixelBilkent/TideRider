using System;

public static class TimeManager
{
    /// <summary>
    /// Gets the current time formatted as a string.
    /// </summary>
    public static string GetCurrentTimeString()
    {
        return DateTime.Now.ToString();
    }

    /// <summary>
    /// Calculates how much time has passed since a saved time string.
    /// </summary>
    public static TimeSpan GetTimePassed(string savedTimeString)
    {
        if (string.IsNullOrEmpty(savedTimeString))
            return TimeSpan.MaxValue; // If no time was saved, assume a very long time has passed

        if (DateTime.TryParse(savedTimeString, out DateTime savedTime))
        {
            return DateTime.Now - savedTime;
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
    /// Returns a formatted string (MM:SS) for remaining cooldown time.
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