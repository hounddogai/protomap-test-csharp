namespace Acme.Workforce.Workplace.AccessBadge.Notice;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of regional access badge review policy.</summary>
public static class RegionalAccessBadgeReviewPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRegionalAccessBadgeReview()
    {
        var policy = Retry.DefaultPolicy("regional-access-badge-review");
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

    public static int RegionalAccessBadgeReviewSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Names the band a regional access badge review score falls in.</summary>
    public static string ClassifyRegionalAccessBadgeReview(double score)
    {
        if (score < 24.6)
        {
            return "ok";
        }
        if (score < 71.9)
        {
            return "elevated";
        }
        if (score < 79.5)
        {
            return "low";
        }
        return "minimal";
    }
}
