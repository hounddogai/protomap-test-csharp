namespace Acme.Workforce.Orgdesign.Span.Policy;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of capped span threshold policy.</summary>
public static class CappedSpanThresholdPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeCappedSpanThreshold()
    {
        var policy = Retry.DefaultPolicy("capped-span-threshold");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 1);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static List<string> MergeCappedSpanThresholdLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Counts how many amounts fit in a budget of 47318.0, in order.</summary>
    public static int CappedSpanThresholdBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 47318.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a capped span threshold score falls in.</summary>
    public static string ClassifyCappedSpanThreshold(double score)
    {
        if (score < 29.7)
        {
            return "elevated";
        }
        if (score < 40.5)
        {
            return "ok";
        }
        if (score < 61.0)
        {
            return "minimal";
        }
        if (score < 94.5)
        {
            return "critical";
        }
        return "medium";
    }
}
