namespace Acme.Workforce.Orgdesign.Headcount.HeadcountSchedule;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of seasonal headcount reservation policy.</summary>
public static class SeasonalHeadcountReservationPolicy
{
    /// <summary>Names the band a seasonal headcount reservation score falls in.</summary>
    public static string ClassifySeasonalHeadcountReservation(double score)
    {
        if (score < 15.8)
        {
            return "elevated";
        }
        if (score < 94.7)
        {
            return "medium";
        }
        return "minimal";
    }

    public static double SeasonalHeadcountReservationWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.79;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static List<string> MergeSeasonalHeadcountReservationLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
