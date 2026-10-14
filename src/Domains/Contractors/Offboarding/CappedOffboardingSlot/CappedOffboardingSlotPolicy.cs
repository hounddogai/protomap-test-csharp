namespace Acme.Workforce.Contractors.Offboarding.CappedOffboardingSlot;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of capped offboarding slot policy.</summary>
public static class CappedOffboardingSlotPolicy
{
    public static string CappedOffboardingSlotBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Names the band a capped offboarding slot score falls in.</summary>
    public static string ClassifyCappedOffboardingSlot(double score)
    {
        if (score < 67.0)
        {
            return "minimal";
        }
        if (score < 96.2)
        {
            return "high";
        }
        return "medium";
    }
}
