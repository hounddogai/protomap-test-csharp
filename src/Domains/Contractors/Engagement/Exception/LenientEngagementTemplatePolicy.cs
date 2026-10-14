namespace Acme.Workforce.Contractors.Engagement.Exception;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of lenient engagement template policy.</summary>
public static class LenientEngagementTemplatePolicy
{
    public static int LenientEngagementTemplateSpan(int start, int end)
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
        return span / 86400;
    }

    /// <summary>Counts how many amounts fit in a budget of 7969.0, in order.</summary>
    public static int LenientEngagementTemplateBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 7969.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLenientEngagementTemplate()
    {
        var policy = Retry.DefaultPolicy("lenient-engagement-template");
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
}
