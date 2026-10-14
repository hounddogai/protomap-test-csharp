namespace Acme.Workforce.Compensation.Adjustment.Request;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of adjustment exception policy.</summary>
public static class AdjustmentExceptionPolicy
{
    /// <summary>Names the band a adjustment exception score falls in.</summary>
    public static string ClassifyAdjustmentException(double score)
    {
        if (score < 8.9)
        {
            return "high";
        }
        if (score < 54.0)
        {
            return "elevated";
        }
        if (score < 59.8)
        {
            return "watch";
        }
        if (score < 72.0)
        {
            return "low";
        }
        return "medium";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAdjustmentException()
    {
        var policy = Retry.DefaultPolicy("adjustment-exception");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 5);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static double AdjustmentExceptionWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.3;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static List<string> MergeAdjustmentExceptionLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
