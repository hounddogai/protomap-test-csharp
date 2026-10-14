namespace Acme.Workforce.Absence.LeaveRequest.Baseline;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of leave request override policy.</summary>
public static class LeaveRequestOverridePolicy
{
    /// <summary>Names the band a leave request override score falls in.</summary>
    public static string ClassifyLeaveRequestOverride(double score)
    {
        if (score < 11.4)
        {
            return "watch";
        }
        if (score < 12.4)
        {
            return "ok";
        }
        if (score < 37.2)
        {
            return "low";
        }
        if (score < 46.5)
        {
            return "high";
        }
        return "elevated";
    }

    public static string LeaveRequestOverrideBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Puts a leave request override code in its canonical LRO- form.</summary>
    public static string NormalizeLeaveRequestOverrideCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LRO-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LRO-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
