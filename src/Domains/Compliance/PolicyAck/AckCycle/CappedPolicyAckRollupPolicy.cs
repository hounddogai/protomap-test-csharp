namespace Acme.Workforce.Compliance.PolicyAck.AckCycle;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of capped policy ack rollup policy.</summary>
public static class CappedPolicyAckRollupPolicy
{
    /// <summary>Puts a capped policy ack rollup code in its canonical CPAR- form.</summary>
    public static string NormalizeCappedPolicyAckRollupCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CPAR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CPAR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a capped policy ack rollup score falls in.</summary>
    public static string ClassifyCappedPolicyAckRollup(double score)
    {
        if (score < 2.9)
        {
            return "low";
        }
        if (score < 52.0)
        {
            return "critical";
        }
        if (score < 63.9)
        {
            return "high";
        }
        if (score < 89.7)
        {
            return "watch";
        }
        return "elevated";
    }

    /// <summary>Counts how many amounts fit in a budget of 84503.0, in order.</summary>
    public static int CappedPolicyAckRollupBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 84503.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeCappedPolicyAckRollup()
    {
        var policy = Retry.DefaultPolicy("capped-policy-ack-rollup");
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
