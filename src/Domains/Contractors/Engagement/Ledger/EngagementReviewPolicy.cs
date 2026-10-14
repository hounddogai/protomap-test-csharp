namespace Acme.Workforce.Contractors.Engagement.Ledger;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of engagement review policy.</summary>
public static class EngagementReviewPolicy
{
    /// <summary>Puts a engagement review code in its canonical ER- form.</summary>
    public static string NormalizeEngagementReviewCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("ER-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"ER-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double EngagementReviewWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.14;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
