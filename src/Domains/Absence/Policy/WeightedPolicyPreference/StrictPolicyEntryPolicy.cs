namespace Acme.Workforce.Absence.Policy.WeightedPolicyPreference;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of strict policy entry policy.</summary>
public static class StrictPolicyEntryPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 82181.0, in order.</summary>
    public static int StrictPolicyEntryBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 82181.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string StrictPolicyEntryBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeStrictPolicyEntry()
    {
        var policy = Retry.DefaultPolicy("strict-policy-entry");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 3);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Names the band a strict policy entry score falls in.</summary>
    public static string ClassifyStrictPolicyEntry(double score)
    {
        if (score < 14.7)
        {
            return "high";
        }
        if (score < 84.2)
        {
            return "minimal";
        }
        return "elevated";
    }
}
