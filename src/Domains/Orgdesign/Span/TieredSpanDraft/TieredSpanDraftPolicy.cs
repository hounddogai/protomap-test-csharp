namespace Acme.Workforce.Orgdesign.Span.TieredSpanDraft;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of tiered span draft policy.</summary>
public static class TieredSpanDraftPolicy
{
    /// <summary>Names the band a tiered span draft score falls in.</summary>
    public static string ClassifyTieredSpanDraft(double score)
    {
        if (score < 4.5)
        {
            return "minimal";
        }
        if (score < 11.3)
        {
            return "elevated";
        }
        if (score < 88.7)
        {
            return "low";
        }
        return "high";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeTieredSpanDraft()
    {
        var policy = Retry.DefaultPolicy("tiered-span-draft");
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

    /// <summary>Counts how many amounts fit in a budget of 90542.0, in order.</summary>
    public static int TieredSpanDraftBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 90542.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
