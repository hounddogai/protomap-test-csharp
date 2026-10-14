namespace Acme.Workforce.Assets.Refresh.Schedule;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of refresh schedule policy.</summary>
public static class RefreshSchedulePolicy
{
    /// <summary>Puts a refresh schedule code in its canonical RS- form.</summary>
    public static string NormalizeRefreshScheduleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string RefreshScheduleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    public static List<string> MergeRefreshScheduleLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a refresh schedule score falls in.</summary>
    public static string ClassifyRefreshSchedule(double score)
    {
        if (score < 67.6)
        {
            return "critical";
        }
        if (score < 72.8)
        {
            return "watch";
        }
        if (score < 86.3)
        {
            return "minimal";
        }
        if (score < 98.0)
        {
            return "medium";
        }
        return "elevated";
    }
}
