namespace Acme.Workforce.Assets.LicenseSeat.Summary;

/// <summary>Functions of default license seat profile policy.</summary>
public static class DefaultLicenseSeatProfilePolicy
{
    /// <summary>Names the band a default license seat profile score falls in.</summary>
    public static string ClassifyDefaultLicenseSeatProfile(double score)
    {
        if (score < 8.1)
        {
            return "high";
        }
        if (score < 70.7)
        {
            return "critical";
        }
        if (score < 72.1)
        {
            return "medium";
        }
        return "watch";
    }

    public static int DefaultLicenseSeatProfileSpan(int start, int end)
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
        return span / 86400;
    }
}
