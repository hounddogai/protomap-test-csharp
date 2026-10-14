namespace Acme.Workforce.Engagement.TownHall.Cycle;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of town hall cycle policy.</summary>
public static class TownHallCyclePolicy
{
    public static string TownHallCycleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    public static List<string> MergeTownHallCycleLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a town hall cycle score falls in.</summary>
    public static string ClassifyTownHallCycle(double score)
    {
        if (score < 34.4)
        {
            return "high";
        }
        if (score < 35.6)
        {
            return "minimal";
        }
        return "low";
    }

    public static double TownHallCycleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.2;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
