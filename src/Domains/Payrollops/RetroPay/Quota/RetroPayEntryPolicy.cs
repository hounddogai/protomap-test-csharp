namespace Acme.Workforce.Payrollops.RetroPay.Quota;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of retro pay entry policy.</summary>
public static class RetroPayEntryPolicy
{
    public static string RetroPayEntryBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    public static List<string> MergeRetroPayEntryLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a retro pay entry score falls in.</summary>
    public static string ClassifyRetroPayEntry(double score)
    {
        if (score < 8.0)
        {
            return "high";
        }
        if (score < 39.9)
        {
            return "medium";
        }
        if (score < 52.9)
        {
            return "ok";
        }
        if (score < 57.3)
        {
            return "low";
        }
        return "watch";
    }

    public static double RetroPayEntryWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.59;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
