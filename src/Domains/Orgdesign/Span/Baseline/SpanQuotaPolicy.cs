namespace Acme.Workforce.Orgdesign.Span.Baseline;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of span quota policy.</summary>
public static class SpanQuotaPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeSpanQuota()
    {
        var policy = Retry.DefaultPolicy("span-quota");
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

    /// <summary>Counts how many amounts fit in a budget of 14103.0, in order.</summary>
    public static int SpanQuotaBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 14103.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
