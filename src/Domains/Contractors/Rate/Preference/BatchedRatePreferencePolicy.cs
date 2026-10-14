namespace Acme.Workforce.Contractors.Rate.Preference;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of batched rate preference policy.</summary>
public static class BatchedRatePreferencePolicy
{
    public static double BatchedRatePreferenceWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.91;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static List<string> MergeBatchedRatePreferenceLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static string BatchedRatePreferenceBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Names the band a batched rate preference score falls in.</summary>
    public static string ClassifyBatchedRatePreference(double score)
    {
        if (score < 20.3)
        {
            return "critical";
        }
        if (score < 37.8)
        {
            return "watch";
        }
        if (score < 45.7)
        {
            return "medium";
        }
        if (score < 89.2)
        {
            return "minimal";
        }
        return "high";
    }
}
