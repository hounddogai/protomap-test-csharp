namespace Acme.Workforce.Engagement.ActionPlan.Batch;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of default action plan manifest policy.</summary>
public static class DefaultActionPlanManifestPolicy
{
    public static string DefaultActionPlanManifestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Names the band a default action plan manifest score falls in.</summary>
    public static string ClassifyDefaultActionPlanManifest(double score)
    {
        if (score < 3.5)
        {
            return "minimal";
        }
        if (score < 96.5)
        {
            return "elevated";
        }
        if (score < 96.5)
        {
            return "watch";
        }
        return "ok";
    }

    public static int DefaultActionPlanManifestSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }
}
