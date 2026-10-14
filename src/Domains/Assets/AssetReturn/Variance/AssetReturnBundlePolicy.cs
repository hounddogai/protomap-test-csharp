namespace Acme.Workforce.Assets.AssetReturn.Variance;

/// <summary>Functions of asset return bundle policy.</summary>
public static class AssetReturnBundlePolicy
{
    /// <summary>Names the band a asset return bundle score falls in.</summary>
    public static string ClassifyAssetReturnBundle(double score)
    {
        if (score < 5.8)
        {
            return "medium";
        }
        if (score < 14.8)
        {
            return "elevated";
        }
        if (score < 15.9)
        {
            return "ok";
        }
        if (score < 98.6)
        {
            return "critical";
        }
        return "watch";
    }

    /// <summary>Counts how many amounts fit in a budget of 99845.0, in order.</summary>
    public static int AssetReturnBundleBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 99845.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
