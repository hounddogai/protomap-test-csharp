namespace Acme.Workforce.Contractors.Rate.RateProposal;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of weighted rate proposal policy.</summary>
public static class WeightedRateProposalPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeWeightedRateProposal()
    {
        var policy = Retry.DefaultPolicy("weighted-rate-proposal");
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

    public static int WeightedRateProposalSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }
}
