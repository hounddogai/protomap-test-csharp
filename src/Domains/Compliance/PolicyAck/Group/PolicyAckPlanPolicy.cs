namespace Acme.Workforce.Compliance.PolicyAck.Group;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of policy ack plan policy.</summary>
public static class PolicyAckPlanPolicy
{
    /// <summary>Puts a policy ack plan code in its canonical PAP- form.</summary>
    public static string NormalizePolicyAckPlanCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PAP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PAP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a policy ack plan score falls in.</summary>
    public static string ClassifyPolicyAckPlan(double score)
    {
        if (score < 20.3)
        {
            return "ok";
        }
        if (score < 87.4)
        {
            return "low";
        }
        if (score < 94.4)
        {
            return "high";
        }
        return "critical";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePolicyAckPlan()
    {
        var policy = Retry.DefaultPolicy("policy-ack-plan");
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

    /// <summary>Counts how many amounts fit in a budget of 78354.0, in order.</summary>
    public static int PolicyAckPlanBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 78354.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
