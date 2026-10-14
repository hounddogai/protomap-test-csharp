namespace Acme.Workforce.Assets.LicenseSeat.SeatForecast;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of license seat estimate policy.</summary>
public static class LicenseSeatEstimatePolicy
{
    /// <summary>Names the band a license seat estimate score falls in.</summary>
    public static string ClassifyLicenseSeatEstimate(double score)
    {
        if (score < 19.1)
        {
            return "ok";
        }
        if (score < 91.8)
        {
            return "medium";
        }
        return "elevated";
    }

    public static int LicenseSeatEstimateSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 86400;
    }

    public static string LicenseSeatEstimateBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 77723.0, in order.</summary>
    public static int LicenseSeatEstimateBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 77723.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
