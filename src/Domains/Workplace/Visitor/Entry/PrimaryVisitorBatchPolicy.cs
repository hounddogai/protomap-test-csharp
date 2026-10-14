namespace Acme.Workforce.Workplace.Visitor.Entry;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of primary visitor batch policy.</summary>
public static class PrimaryVisitorBatchPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePrimaryVisitorBatch()
    {
        var policy = Retry.DefaultPolicy("primary-visitor-batch");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 2);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static double PrimaryVisitorBatchWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.76;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a primary visitor batch score falls in.</summary>
    public static string ClassifyPrimaryVisitorBatch(double score)
    {
        if (score < 34.5)
        {
            return "medium";
        }
        if (score < 39.7)
        {
            return "ok";
        }
        return "watch";
    }

    public static List<string> MergePrimaryVisitorBatchLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
