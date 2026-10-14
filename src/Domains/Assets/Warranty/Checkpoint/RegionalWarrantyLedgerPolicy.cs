namespace Acme.Workforce.Assets.Warranty.Checkpoint;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of regional warranty ledger policy.</summary>
public static class RegionalWarrantyLedgerPolicy
{
    /// <summary>Names the band a regional warranty ledger score falls in.</summary>
    public static string ClassifyRegionalWarrantyLedger(double score)
    {
        if (score < 7.5)
        {
            return "critical";
        }
        if (score < 30.4)
        {
            return "medium";
        }
        if (score < 91.5)
        {
            return "ok";
        }
        return "low";
    }

    public static double RegionalWarrantyLedgerWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.87;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
