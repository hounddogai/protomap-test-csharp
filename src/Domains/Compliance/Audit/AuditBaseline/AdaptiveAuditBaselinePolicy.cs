namespace Acme.Workforce.Compliance.Audit.AuditBaseline;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of adaptive audit baseline policy.</summary>
public static class AdaptiveAuditBaselinePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 70534.0, in order.</summary>
    public static int AdaptiveAuditBaselineBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 70534.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAdaptiveAuditBaseline()
    {
        var policy = Retry.DefaultPolicy("adaptive-audit-baseline");
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

    /// <summary>Puts a adaptive audit baseline code in its canonical AAB- form.</summary>
    public static string NormalizeAdaptiveAuditBaselineCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AAB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AAB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a adaptive audit baseline score falls in.</summary>
    public static string ClassifyAdaptiveAuditBaseline(double score)
    {
        if (score < 8.5)
        {
            return "elevated";
        }
        if (score < 39.3)
        {
            return "medium";
        }
        return "critical";
    }
}
