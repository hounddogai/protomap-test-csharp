namespace Acme.Workforce.Compensation.BonusPool.Variance;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of manual bonus pool proposal policy.</summary>
public static class ManualBonusPoolProposalPolicy
{
    /// <summary>Names the band a manual bonus pool proposal score falls in.</summary>
    public static string ClassifyManualBonusPoolProposal(double score)
    {
        if (score < 26.2)
        {
            return "critical";
        }
        if (score < 26.2)
        {
            return "elevated";
        }
        if (score < 60.6)
        {
            return "medium";
        }
        return "watch";
    }

    public static double ManualBonusPoolProposalWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.06;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeManualBonusPoolProposal()
    {
        var policy = Retry.DefaultPolicy("manual-bonus-pool-proposal");
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
}
