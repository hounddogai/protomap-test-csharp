namespace Acme.Workforce.Absence.LeaveRequest.Plan;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of leave request ledger policy.</summary>
public static class LeaveRequestLedgerPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 96840.0, in order.</summary>
    public static int LeaveRequestLedgerBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 96840.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLeaveRequestLedger()
    {
        var policy = Retry.DefaultPolicy("leave-request-ledger");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 4);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Puts a leave request ledger code in its canonical LRL- form.</summary>
    public static string NormalizeLeaveRequestLedgerCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LRL-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LRL-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
