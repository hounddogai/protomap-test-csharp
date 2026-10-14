namespace Acme.Workforce.Assets.Refresh.Rollup;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of refresh variance policy.</summary>
public static class RefreshVariancePolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRefreshVariance()
    {
        var policy = Retry.DefaultPolicy("refresh-variance");
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

    /// <summary>Counts how many amounts fit in a budget of 93465.0, in order.</summary>
    public static int RefreshVarianceBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 93465.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a refresh variance code in its canonical RV- form.</summary>
    public static string NormalizeRefreshVarianceCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RV-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RV-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
