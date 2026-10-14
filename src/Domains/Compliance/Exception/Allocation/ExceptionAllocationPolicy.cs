namespace Acme.Workforce.Compliance.Exception.Allocation;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of exception allocation policy.</summary>
public static class ExceptionAllocationPolicy
{
    public static int ExceptionAllocationSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 86400;
    }

    /// <summary>Names the band a exception allocation score falls in.</summary>
    public static string ClassifyExceptionAllocation(double score)
    {
        if (score < 7.0)
        {
            return "medium";
        }
        if (score < 18.5)
        {
            return "high";
        }
        if (score < 25.0)
        {
            return "minimal";
        }
        if (score < 33.4)
        {
            return "ok";
        }
        return "watch";
    }

    public static List<string> MergeExceptionAllocationLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double ExceptionAllocationWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.28;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
