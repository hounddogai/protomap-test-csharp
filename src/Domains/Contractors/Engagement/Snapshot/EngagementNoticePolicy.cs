namespace Acme.Workforce.Contractors.Engagement.Snapshot;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of engagement notice policy.</summary>
public static class EngagementNoticePolicy
{
    /// <summary>Names the band a engagement notice score falls in.</summary>
    public static string ClassifyEngagementNotice(double score)
    {
        if (score < 8.9)
        {
            return "watch";
        }
        if (score < 53.8)
        {
            return "minimal";
        }
        return "critical";
    }

    /// <summary>Puts a engagement notice code in its canonical EN- form.</summary>
    public static string NormalizeEngagementNoticeCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("EN-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"EN-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string EngagementNoticeBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
