namespace Acme.Workforce.Assets.Warranty.Ledger;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of tiered warranty tier policy.</summary>
public static class TieredWarrantyTierPolicy
{
    public static string TieredWarrantyTierBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    public static double TieredWarrantyTierWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.6;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
