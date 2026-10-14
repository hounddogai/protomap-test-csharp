namespace Acme.Workforce.Compensation.BonusPool.Tier;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of bonus pool proposal policy.</summary>
public static class BonusPoolProposalPolicy
{
    /// <summary>Names the band a bonus pool proposal score falls in.</summary>
    public static string ClassifyBonusPoolProposal(double score)
    {
        if (score < 52.0)
        {
            return "elevated";
        }
        if (score < 56.2)
        {
            return "low";
        }
        if (score < 65.4)
        {
            return "medium";
        }
        return "watch";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeBonusPoolProposal()
    {
        var policy = Retry.DefaultPolicy("bonus-pool-proposal");
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

    /// <summary>Puts a bonus pool proposal code in its canonical BPP- form.</summary>
    public static string NormalizeBonusPoolProposalCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BPP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BPP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string BonusPoolProposalBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }
}
