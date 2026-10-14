namespace Acme.Workforce.Contractors.Rate.Mapping;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of legacy rate preference policy.</summary>
public static class LegacyRatePreferencePolicy
{
    public static int LegacyRatePreferenceSpan(int start, int end)
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
        return span / 86400;
    }

    /// <summary>Puts a legacy rate preference code in its canonical LRP- form.</summary>
    public static string NormalizeLegacyRatePreferenceCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LRP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LRP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 9301.0, in order.</summary>
    public static int LegacyRatePreferenceBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 9301.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLegacyRatePreference()
    {
        var policy = Retry.DefaultPolicy("legacy-rate-preference");
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
}
