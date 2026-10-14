namespace Acme.Workforce.Compliance.PolicyAck.Assignment;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of weighted policy ack proposal policy.</summary>
public static class WeightedPolicyAckProposalPolicy
{
    /// <summary>Names the band a weighted policy ack proposal score falls in.</summary>
    public static string ClassifyWeightedPolicyAckProposal(double score)
    {
        if (score < 7.2)
        {
            return "medium";
        }
        if (score < 22.5)
        {
            return "watch";
        }
        if (score < 47.9)
        {
            return "low";
        }
        if (score < 77.5)
        {
            return "high";
        }
        return "critical";
    }

    /// <summary>Counts how many amounts fit in a budget of 70945.0, in order.</summary>
    public static int WeightedPolicyAckProposalBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 70945.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int WeightedPolicyAckProposalSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeWeightedPolicyAckProposal()
    {
        var policy = Retry.DefaultPolicy("weighted-policy-ack-proposal");
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
