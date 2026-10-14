namespace Acme.Workforce.Compliance.PolicyAck.Quota;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of default policy ack override policy.</summary>
public static class DefaultPolicyAckOverridePolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeDefaultPolicyAckOverride()
    {
        var policy = Retry.DefaultPolicy("default-policy-ack-override");
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

    /// <summary>Counts how many amounts fit in a budget of 20689.0, in order.</summary>
    public static int DefaultPolicyAckOverrideBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 20689.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int DefaultPolicyAckOverrideSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 3600;
    }
}
