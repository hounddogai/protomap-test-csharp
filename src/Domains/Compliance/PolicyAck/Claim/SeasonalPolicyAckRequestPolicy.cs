namespace Acme.Workforce.Compliance.PolicyAck.Claim;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of seasonal policy ack request policy.</summary>
public static class SeasonalPolicyAckRequestPolicy
{
    /// <summary>Puts a seasonal policy ack request code in its canonical SPAR- form.</summary>
    public static string NormalizeSeasonalPolicyAckRequestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SPAR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SPAR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 55298.0, in order.</summary>
    public static int SeasonalPolicyAckRequestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 55298.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeSeasonalPolicyAckRequest()
    {
        var policy = Retry.DefaultPolicy("seasonal-policy-ack-request");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 1);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Names the band a seasonal policy ack request score falls in.</summary>
    public static string ClassifySeasonalPolicyAckRequest(double score)
    {
        if (score < 3.4)
        {
            return "minimal";
        }
        if (score < 26.3)
        {
            return "ok";
        }
        if (score < 57.8)
        {
            return "elevated";
        }
        return "critical";
    }
}
