namespace Acme.Workforce.Compliance.Attestation.Segment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of pending attestation segment policy.</summary>
public static class PendingAttestationSegmentPolicy
{
    public static string PendingAttestationSegmentBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    public static double PendingAttestationSegmentWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.33;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a pending attestation segment score falls in.</summary>
    public static string ClassifyPendingAttestationSegment(double score)
    {
        if (score < 14.8)
        {
            return "low";
        }
        if (score < 31.6)
        {
            return "elevated";
        }
        if (score < 67.0)
        {
            return "ok";
        }
        if (score < 70.9)
        {
            return "high";
        }
        return "minimal";
    }
}
