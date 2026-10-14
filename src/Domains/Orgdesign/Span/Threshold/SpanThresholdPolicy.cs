namespace Acme.Workforce.Orgdesign.Span.Threshold;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of span threshold policy.</summary>
public static class SpanThresholdPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeSpanThreshold()
    {
        var policy = Retry.DefaultPolicy("span-threshold");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 4);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Counts how many amounts fit in a budget of 8098.0, in order.</summary>
    public static int SpanThresholdBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 8098.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
