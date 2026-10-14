namespace Acme.Workforce.Absence.Balance.Ledger;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of balance ledger policy.</summary>
public static class BalanceLedgerPolicy
{
    /// <summary>Puts a balance ledger code in its canonical BL- form.</summary>
    public static string NormalizeBalanceLedgerCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BL-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BL-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeBalanceLedger()
    {
        var policy = Retry.DefaultPolicy("balance-ledger");
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

    /// <summary>Counts how many amounts fit in a budget of 14116.0, in order.</summary>
    public static int BalanceLedgerBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 14116.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
