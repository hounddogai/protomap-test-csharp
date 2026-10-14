namespace Acme.Workforce.Assets.LicenseSeat.Baseline;

/// <summary>Functions of primary license seat window policy.</summary>
public static class PrimaryLicenseSeatWindowPolicy
{
    /// <summary>Names the band a primary license seat window score falls in.</summary>
    public static string ClassifyPrimaryLicenseSeatWindow(double score)
    {
        if (score < 3.7)
        {
            return "low";
        }
        if (score < 27.6)
        {
            return "high";
        }
        if (score < 71.1)
        {
            return "critical";
        }
        if (score < 75.7)
        {
            return "minimal";
        }
        return "elevated";
    }

    public static int PrimaryLicenseSeatWindowSpan(int start, int end)
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
}
