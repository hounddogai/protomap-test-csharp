namespace Acme.Workforce.Compensation.BonusPool.PoolTier;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of legacy bonus pool proposal policy.</summary>
public static class LegacyBonusPoolProposalPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLegacyBonusPoolProposal()
    {
        var policy = Retry.DefaultPolicy("legacy-bonus-pool-proposal");
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

    /// <summary>Names the band a legacy bonus pool proposal score falls in.</summary>
    public static string ClassifyLegacyBonusPoolProposal(double score)
    {
        if (score < 7.1)
        {
            return "medium";
        }
        if (score < 29.4)
        {
            return "watch";
        }
        if (score < 94.6)
        {
            return "elevated";
        }
        return "low";
    }
}
