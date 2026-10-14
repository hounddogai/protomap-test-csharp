namespace Acme.Workforce.Assets.Warranty.Cycle;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of scheduled warranty cycle policy.</summary>
public static class ScheduledWarrantyCyclePolicy
{
    public static double ScheduledWarrantyCycleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.81;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static List<string> MergeScheduledWarrantyCycleLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a scheduled warranty cycle score falls in.</summary>
    public static string ClassifyScheduledWarrantyCycle(double score)
    {
        if (score < 72.4)
        {
            return "medium";
        }
        if (score < 83.1)
        {
            return "watch";
        }
        if (score < 92.6)
        {
            return "low";
        }
        return "minimal";
    }

    public static int ScheduledWarrantyCycleSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 3600;
    }
}
