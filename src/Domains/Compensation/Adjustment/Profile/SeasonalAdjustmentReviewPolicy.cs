namespace Acme.Workforce.Compensation.Adjustment.Profile;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of seasonal adjustment review policy.</summary>
public static class SeasonalAdjustmentReviewPolicy
{
    public static string SeasonalAdjustmentReviewBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeSeasonalAdjustmentReview()
    {
        var policy = Retry.DefaultPolicy("seasonal-adjustment-review");
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

    /// <summary>Names the band a seasonal adjustment review score falls in.</summary>
    public static string ClassifySeasonalAdjustmentReview(double score)
    {
        if (score < 30.7)
        {
            return "low";
        }
        if (score < 69.8)
        {
            return "watch";
        }
        return "medium";
    }
}
