namespace Acme.Workforce.Contractors.Rate.RateProposal;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of weighted rate preference policy.</summary>
public static class WeightedRatePreferencePolicy
{
    /// <summary>Names the band a weighted rate preference score falls in.</summary>
    public static string ClassifyWeightedRatePreference(double score)
    {
        if (score < 34.3)
        {
            return "critical";
        }
        if (score < 61.8)
        {
            return "watch";
        }
        if (score < 80.6)
        {
            return "high";
        }
        if (score < 94.6)
        {
            return "low";
        }
        return "elevated";
    }

    /// <summary>Puts a weighted rate preference code in its canonical WRP- form.</summary>
    public static string NormalizeWeightedRatePreferenceCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("WRP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"WRP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeWeightedRatePreference()
    {
        var policy = Retry.DefaultPolicy("weighted-rate-preference");
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

    /// <summary>Counts how many amounts fit in a budget of 44479.0, in order.</summary>
    public static int WeightedRatePreferenceBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 44479.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
