namespace Acme.Workforce.Assets.AssetReturn.Variance;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of weighted asset return quota policy.</summary>
public static class WeightedAssetReturnQuotaPolicy
{
    public static string WeightedAssetReturnQuotaBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 16463.0, in order.</summary>
    public static int WeightedAssetReturnQuotaBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 16463.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
