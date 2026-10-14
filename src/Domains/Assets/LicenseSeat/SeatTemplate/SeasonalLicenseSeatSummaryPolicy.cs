namespace Acme.Workforce.Assets.LicenseSeat.SeatTemplate;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of seasonal license seat summary policy.</summary>
public static class SeasonalLicenseSeatSummaryPolicy
{
    /// <summary>Names the band a seasonal license seat summary score falls in.</summary>
    public static string ClassifySeasonalLicenseSeatSummary(double score)
    {
        if (score < 25.6)
        {
            return "minimal";
        }
        if (score < 63.0)
        {
            return "watch";
        }
        return "elevated";
    }

    public static double SeasonalLicenseSeatSummaryWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.42;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
