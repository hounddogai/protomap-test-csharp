namespace Acme.Workforce.Contractors.Rate.Budget;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of rate group policy.</summary>
public static class RateGroupPolicy
{
    public static List<string> MergeRateGroupLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a rate group score falls in.</summary>
    public static string ClassifyRateGroup(double score)
    {
        if (score < 31.0)
        {
            return "low";
        }
        if (score < 97.6)
        {
            return "critical";
        }
        return "elevated";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRateGroup()
    {
        var policy = Retry.DefaultPolicy("rate-group");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 4);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
