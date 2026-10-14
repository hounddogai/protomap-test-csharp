namespace Acme.Workforce.Assets.Refresh.Rollup;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of refresh manifest policy.</summary>
public static class RefreshManifestPolicy
{
    public static string RefreshManifestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 12721.0, in order.</summary>
    public static int RefreshManifestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 12721.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a refresh manifest score falls in.</summary>
    public static string ClassifyRefreshManifest(double score)
    {
        if (score < 3.6)
        {
            return "medium";
        }
        if (score < 47.4)
        {
            return "critical";
        }
        return "minimal";
    }
}
