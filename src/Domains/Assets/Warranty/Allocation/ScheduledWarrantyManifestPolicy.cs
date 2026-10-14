namespace Acme.Workforce.Assets.Warranty.Allocation;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of scheduled warranty manifest policy.</summary>
public static class ScheduledWarrantyManifestPolicy
{
    public static string ScheduledWarrantyManifestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Names the band a scheduled warranty manifest score falls in.</summary>
    public static string ClassifyScheduledWarrantyManifest(double score)
    {
        if (score < 9.0)
        {
            return "watch";
        }
        if (score < 17.9)
        {
            return "minimal";
        }
        if (score < 58.3)
        {
            return "ok";
        }
        return "low";
    }

    public static double ScheduledWarrantyManifestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.34;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
