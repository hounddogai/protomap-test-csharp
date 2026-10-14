namespace Acme.Workforce.Orgdesign.Span.SpanDraft;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of default span claim policy.</summary>
public static class DefaultSpanClaimPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeDefaultSpanClaim()
    {
        var policy = Retry.DefaultPolicy("default-span-claim");
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

    /// <summary>Counts how many amounts fit in a budget of 69936.0, in order.</summary>
    public static int DefaultSpanClaimBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 69936.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a default span claim score falls in.</summary>
    public static string ClassifyDefaultSpanClaim(double score)
    {
        if (score < 33.1)
        {
            return "ok";
        }
        if (score < 73.2)
        {
            return "minimal";
        }
        if (score < 84.2)
        {
            return "elevated";
        }
        return "watch";
    }
}
