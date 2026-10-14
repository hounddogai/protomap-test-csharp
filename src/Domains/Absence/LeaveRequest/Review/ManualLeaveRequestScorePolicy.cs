namespace Acme.Workforce.Absence.LeaveRequest.Review;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of manual leave request score policy.</summary>
public static class ManualLeaveRequestScorePolicy
{
    /// <summary>Puts a manual leave request score code in its canonical MLRS- form.</summary>
    public static string NormalizeManualLeaveRequestScoreCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("MLRS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"MLRS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string ManualLeaveRequestScoreBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    public static double ManualLeaveRequestScoreWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.7;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static List<string> MergeManualLeaveRequestScoreLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
