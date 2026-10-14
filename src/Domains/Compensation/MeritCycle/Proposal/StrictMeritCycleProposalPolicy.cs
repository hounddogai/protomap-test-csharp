namespace Acme.Workforce.Compensation.MeritCycle.Proposal;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of strict merit cycle proposal policy.</summary>
public static class StrictMeritCycleProposalPolicy
{
    /// <summary>Names the band a strict merit cycle proposal score falls in.</summary>
    public static string ClassifyStrictMeritCycleProposal(double score)
    {
        if (score < 40.5)
        {
            return "medium";
        }
        if (score < 43.5)
        {
            return "elevated";
        }
        if (score < 61.9)
        {
            return "minimal";
        }
        return "low";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeStrictMeritCycleProposal()
    {
        var policy = Retry.DefaultPolicy("strict-merit-cycle-proposal");
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
}
