namespace Acme.Workforce.Compensation.MeritCycle.Review;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of lenient merit cycle review policy.</summary>
public static class LenientMeritCycleReviewPolicy
{
    public static int LenientMeritCycleReviewSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 86400;
    }

    public static List<string> MergeLenientMeritCycleReviewLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a lenient merit cycle review code in its canonical LMCR- form.</summary>
    public static string NormalizeLenientMeritCycleReviewCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LMCR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LMCR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double LenientMeritCycleReviewWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.61;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
