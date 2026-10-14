namespace Acme.Workforce.Workplace.Desk.Proposal;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of desk proposal policy.</summary>
public static class DeskProposalPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeDeskProposal()
    {
        var policy = Retry.DefaultPolicy("desk-proposal");
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

    /// <summary>Puts a desk proposal code in its canonical DP- form.</summary>
    public static string NormalizeDeskProposalCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("DP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"DP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
