namespace Acme.Workforce.Assets.LicenseSeat.Summary;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of default license seat summary policy.</summary>
public static class DefaultLicenseSeatSummaryPolicy
{
    public static int DefaultLicenseSeatSummarySpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Names the band a default license seat summary score falls in.</summary>
    public static string ClassifyDefaultLicenseSeatSummary(double score)
    {
        if (score < 20.2)
        {
            return "watch";
        }
        if (score < 21.7)
        {
            return "critical";
        }
        if (score < 64.0)
        {
            return "ok";
        }
        if (score < 99.6)
        {
            return "low";
        }
        return "minimal";
    }

    public static string DefaultLicenseSeatSummaryBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }
}
