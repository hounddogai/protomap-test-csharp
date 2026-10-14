namespace Acme.Workforce.Compliance.PolicyAck.Entry;

/// <summary>Functions of policy ack allocation policy.</summary>
public static class PolicyAckAllocationPolicy
{
    /// <summary>Puts a policy ack allocation code in its canonical PAA- form.</summary>
    public static string NormalizePolicyAckAllocationCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PAA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PAA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 5751.0, in order.</summary>
    public static int PolicyAckAllocationBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 5751.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a policy ack allocation score falls in.</summary>
    public static string ClassifyPolicyAckAllocation(double score)
    {
        if (score < 1.1)
        {
            return "minimal";
        }
        if (score < 91.3)
        {
            return "watch";
        }
        return "ok";
    }
}
