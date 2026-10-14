namespace Acme.Workforce.Compliance.Attestation.AttestationSchedule;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of regional attestation schedule policy.</summary>
public static class RegionalAttestationSchedulePolicy
{
    /// <summary>Names the band a regional attestation schedule score falls in.</summary>
    public static string ClassifyRegionalAttestationSchedule(double score)
    {
        if (score < 42.5)
        {
            return "low";
        }
        if (score < 48.2)
        {
            return "watch";
        }
        return "minimal";
    }

    public static double RegionalAttestationScheduleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.11;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
