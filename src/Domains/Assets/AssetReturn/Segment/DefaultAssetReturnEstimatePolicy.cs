namespace Acme.Workforce.Assets.AssetReturn.Segment;

/// <summary>Functions of default asset return estimate policy.</summary>
public static class DefaultAssetReturnEstimatePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 8475.0, in order.</summary>
    public static int DefaultAssetReturnEstimateBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 8475.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a default asset return estimate score falls in.</summary>
    public static string ClassifyDefaultAssetReturnEstimate(double score)
    {
        if (score < 6.4)
        {
            return "medium";
        }
        if (score < 66.8)
        {
            return "high";
        }
        return "watch";
    }
}
