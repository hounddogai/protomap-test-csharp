namespace Acme.Workforce.Assets.Refresh.Window;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of batched refresh window policy.</summary>
public static class BatchedRefreshWindowPolicy
{
    /// <summary>Names the band a batched refresh window score falls in.</summary>
    public static string ClassifyBatchedRefreshWindow(double score)
    {
        if (score < 27.5)
        {
            return "watch";
        }
        if (score < 49.2)
        {
            return "medium";
        }
        if (score < 88.9)
        {
            return "minimal";
        }
        return "low";
    }

    public static string BatchedRefreshWindowBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }
}
