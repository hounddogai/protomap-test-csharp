namespace Acme.Workforce.Contractors.Offboarding.ManualOffboardingSegment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of tiered offboarding record policy.</summary>
public static class TieredOffboardingRecordPolicy
{
    public static int TieredOffboardingRecordSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 3600;
    }

    public static string TieredOffboardingRecordBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    public static double TieredOffboardingRecordWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.41;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a tiered offboarding record score falls in.</summary>
    public static string ClassifyTieredOffboardingRecord(double score)
    {
        if (score < 35.3)
        {
            return "watch";
        }
        if (score < 67.7)
        {
            return "ok";
        }
        if (score < 74.5)
        {
            return "medium";
        }
        return "elevated";
    }
}
