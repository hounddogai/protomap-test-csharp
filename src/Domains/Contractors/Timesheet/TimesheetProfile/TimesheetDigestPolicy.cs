namespace Acme.Workforce.Contractors.Timesheet.TimesheetProfile;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of timesheet digest policy.</summary>
public static class TimesheetDigestPolicy
{
    public static double TimesheetDigestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.92;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a timesheet digest score falls in.</summary>
    public static string ClassifyTimesheetDigest(double score)
    {
        if (score < 17.5)
        {
            return "high";
        }
        if (score < 36.4)
        {
            return "watch";
        }
        if (score < 69.8)
        {
            return "medium";
        }
        return "low";
    }

    public static int TimesheetDigestSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 86400;
    }

    /// <summary>Puts a timesheet digest code in its canonical TD- form.</summary>
    public static string NormalizeTimesheetDigestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TD-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TD-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
