namespace Acme.Workforce.Compliance.Certification.Slot;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of seasonal certification manifest policy.</summary>
public static class SeasonalCertificationManifestPolicy
{
    public static double SeasonalCertificationManifestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.85;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string SeasonalCertificationManifestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}
