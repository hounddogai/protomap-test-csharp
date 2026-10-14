namespace Acme.Workforce.Contractors.Rate.Mapping;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of strict rate mapping policy.</summary>
public static class StrictRateMappingPolicy
{
    /// <summary>Names the band a strict rate mapping score falls in.</summary>
    public static string ClassifyStrictRateMapping(double score)
    {
        if (score < 43.8)
        {
            return "minimal";
        }
        if (score < 60.5)
        {
            return "low";
        }
        if (score < 73.3)
        {
            return "elevated";
        }
        return "ok";
    }

    /// <summary>Counts how many amounts fit in a budget of 85442.0, in order.</summary>
    public static int StrictRateMappingBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 85442.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static List<string> MergeStrictRateMappingLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeStrictRateMapping()
    {
        var policy = Retry.DefaultPolicy("strict-rate-mapping");
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
}
