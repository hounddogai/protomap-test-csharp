namespace Acme.Workforce.Talent.Mobility.Entry;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of mobility entry policy.</summary>
public static class MobilityEntryPolicy
{
    public static string MobilityEntryBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Names the band a mobility entry score falls in.</summary>
    public static string ClassifyMobilityEntry(double score)
    {
        if (score < 55.1)
        {
            return "watch";
        }
        if (score < 70.0)
        {
            return "high";
        }
        return "ok";
    }

    public static double MobilityEntryWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.63;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int MobilityEntrySpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 60;
    }
}
