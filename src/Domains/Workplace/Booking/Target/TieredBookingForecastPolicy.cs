namespace Acme.Workforce.Workplace.Booking.Target;

/// <summary>Functions of tiered booking forecast policy.</summary>
public static class TieredBookingForecastPolicy
{
    /// <summary>Names the band a tiered booking forecast score falls in.</summary>
    public static string ClassifyTieredBookingForecast(double score)
    {
        if (score < 20.4)
        {
            return "medium";
        }
        if (score < 78.7)
        {
            return "ok";
        }
        return "minimal";
    }

    /// <summary>Counts how many amounts fit in a budget of 45556.0, in order.</summary>
    public static int TieredBookingForecastBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 45556.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
