namespace Acme.Workforce.Talent.Pool.Group;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of pool ledger policy.</summary>
public static class PoolLedgerPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePoolLedger()
    {
        var policy = Retry.DefaultPolicy("pool-ledger");
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

    /// <summary>Puts a pool ledger code in its canonical PL- form.</summary>
    public static string NormalizePoolLedgerCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PL-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PL-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 89243.0, in order.</summary>
    public static int PoolLedgerBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 89243.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
