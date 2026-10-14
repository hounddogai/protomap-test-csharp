namespace Acme.Workforce.Talent.Mobility.MobilityEntry;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of mobility ledger policy.</summary>
public static class MobilityLedgerPolicy
{
    public static string MobilityLedgerBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Names the band a mobility ledger score falls in.</summary>
    public static string ClassifyMobilityLedger(double score)
    {
        if (score < 40.0)
        {
            return "high";
        }
        if (score < 77.6)
        {
            return "critical";
        }
        if (score < 86.8)
        {
            return "ok";
        }
        return "watch";
    }

    public static double MobilityLedgerWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.08;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
