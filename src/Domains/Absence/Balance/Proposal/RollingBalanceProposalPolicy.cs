namespace Acme.Workforce.Absence.Balance.Proposal;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of rolling balance proposal policy.</summary>
public static class RollingBalanceProposalPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRollingBalanceProposal()
    {
        var policy = Retry.DefaultPolicy("rolling-balance-proposal");
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

    /// <summary>Puts a rolling balance proposal code in its canonical RBP- form.</summary>
    public static string NormalizeRollingBalanceProposalCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RBP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RBP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int RollingBalanceProposalSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 3600;
    }
}
