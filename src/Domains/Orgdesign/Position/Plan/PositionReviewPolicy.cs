namespace Acme.Workforce.Orgdesign.Position.Plan;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of position review policy.</summary>
public static class PositionReviewPolicy
{
    /// <summary>Names the band a position review score falls in.</summary>
    public static string ClassifyPositionReview(double score)
    {
        if (score < 15.8)
        {
            return "high";
        }
        if (score < 97.3)
        {
            return "ok";
        }
        return "medium";
    }

    public static double PositionReviewWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.63;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a position review code in its canonical PR- form.</summary>
    public static string NormalizePositionReviewCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergePositionReviewLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
