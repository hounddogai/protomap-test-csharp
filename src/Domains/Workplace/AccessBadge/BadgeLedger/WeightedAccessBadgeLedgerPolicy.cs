namespace Acme.Workforce.Workplace.AccessBadge.BadgeLedger;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of weighted access badge ledger policy.</summary>
public static class WeightedAccessBadgeLedgerPolicy
{
    /// <summary>Puts a weighted access badge ledger code in its canonical WABL- form.</summary>
    public static string NormalizeWeightedAccessBadgeLedgerCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("WABL-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"WABL-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeWeightedAccessBadgeLedger()
    {
        var policy = Retry.DefaultPolicy("weighted-access-badge-ledger");
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

    /// <summary>Counts how many amounts fit in a budget of 53206.0, in order.</summary>
    public static int WeightedAccessBadgeLedgerBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 53206.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a weighted access badge ledger score falls in.</summary>
    public static string ClassifyWeightedAccessBadgeLedger(double score)
    {
        if (score < 2.2)
        {
            return "minimal";
        }
        if (score < 78.5)
        {
            return "high";
        }
        return "ok";
    }
}
