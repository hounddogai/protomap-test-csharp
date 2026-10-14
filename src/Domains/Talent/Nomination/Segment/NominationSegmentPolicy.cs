namespace Acme.Workforce.Talent.Nomination.Segment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of nomination segment policy.</summary>
public static class NominationSegmentPolicy
{
    public static double NominationSegmentWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.04;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string NominationSegmentBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
