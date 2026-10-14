namespace Acme.Workforce.Absence.LeaveRequest.Adjustment;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of legacy leave request record policy.</summary>
public static class LegacyLeaveRequestRecordPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLegacyLeaveRequestRecord()
    {
        var policy = Retry.DefaultPolicy("legacy-leave-request-record");
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

    /// <summary>Puts a legacy leave request record code in its canonical LLRR- form.</summary>
    public static string NormalizeLegacyLeaveRequestRecordCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LLRR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LLRR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 70219.0, in order.</summary>
    public static int LegacyLeaveRequestRecordBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 70219.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
