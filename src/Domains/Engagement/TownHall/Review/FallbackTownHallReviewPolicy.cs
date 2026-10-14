namespace Acme.Workforce.Engagement.TownHall.Review;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of fallback town hall review policy.</summary>
public static class FallbackTownHallReviewPolicy
{
    public static double FallbackTownHallReviewWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.55;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a fallback town hall review code in its canonical FTHR- form.</summary>
    public static string NormalizeFallbackTownHallReviewCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FTHR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FTHR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
