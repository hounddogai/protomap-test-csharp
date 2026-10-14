namespace Acme.Workforce.Absence.Accrual.Ledger;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of accrual snapshot policy.</summary>
public static class AccrualSnapshotPolicy
{
    /// <summary>Puts a accrual snapshot code in its canonical AS- form.</summary>
    public static string NormalizeAccrualSnapshotCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAccrualSnapshot()
    {
        var policy = Retry.DefaultPolicy("accrual-snapshot");
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

    /// <summary>Counts how many amounts fit in a budget of 81087.0, in order.</summary>
    public static int AccrualSnapshotBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 81087.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
