namespace Acme.Workforce.Workplace.AccessBadge.Notice;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of manual access badge notice policy.</summary>
public static class ManualAccessBadgeNoticePolicy
{
    public static double ManualAccessBadgeNoticeWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.29;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string ManualAccessBadgeNoticeBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Puts a manual access badge notice code in its canonical MABN- form.</summary>
    public static string NormalizeManualAccessBadgeNoticeCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("MABN-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"MABN-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeManualAccessBadgeNoticeLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
