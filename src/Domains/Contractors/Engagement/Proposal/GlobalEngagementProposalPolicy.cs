namespace Acme.Workforce.Contractors.Engagement.Proposal;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of global engagement proposal policy.</summary>
public static class GlobalEngagementProposalPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeGlobalEngagementProposal()
    {
        var policy = Retry.DefaultPolicy("global-engagement-proposal");
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

    /// <summary>Puts a global engagement proposal code in its canonical GEP- form.</summary>
    public static string NormalizeGlobalEngagementProposalCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("GEP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"GEP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
