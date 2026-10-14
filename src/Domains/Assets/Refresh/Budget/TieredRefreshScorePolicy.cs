namespace Acme.Workforce.Assets.Refresh.Budget;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of tiered refresh score policy.</summary>
public static class TieredRefreshScorePolicy
{
    /// <summary>Names the band a tiered refresh score score falls in.</summary>
    public static string ClassifyTieredRefreshScore(double score)
    {
        if (score < 7.7)
        {
            return "ok";
        }
        if (score < 8.8)
        {
            return "elevated";
        }
        if (score < 54.4)
        {
            return "high";
        }
        return "low";
    }

    public static string TieredRefreshScoreBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }
}
