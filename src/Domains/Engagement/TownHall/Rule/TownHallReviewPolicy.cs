namespace Acme.Workforce.Engagement.TownHall.Rule;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of town hall review policy.</summary>
public static class TownHallReviewPolicy
{
    public static double TownHallReviewWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.73;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a town hall review score falls in.</summary>
    public static string ClassifyTownHallReview(double score)
    {
        if (score < 8.3)
        {
            return "watch";
        }
        if (score < 41.8)
        {
            return "high";
        }
        if (score < 80.2)
        {
            return "critical";
        }
        if (score < 80.9)
        {
            return "elevated";
        }
        return "medium";
    }

    public static List<string> MergeTownHallReviewLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
