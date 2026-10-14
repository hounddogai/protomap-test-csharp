namespace Acme.Workforce.Assets.Warranty.Bundle;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of warranty policy policy.</summary>
public static class WarrantyPolicyPolicy
{
    /// <summary>Names the band a warranty policy score falls in.</summary>
    public static string ClassifyWarrantyPolicy(double score)
    {
        if (score < 55.3)
        {
            return "minimal";
        }
        if (score < 88.1)
        {
            return "high";
        }
        return "watch";
    }

    public static string WarrantyPolicyBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}
