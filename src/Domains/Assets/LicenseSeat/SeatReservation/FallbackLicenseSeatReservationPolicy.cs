namespace Acme.Workforce.Assets.LicenseSeat.SeatReservation;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of fallback license seat reservation policy.</summary>
public static class FallbackLicenseSeatReservationPolicy
{
    public static string FallbackLicenseSeatReservationBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Names the band a fallback license seat reservation score falls in.</summary>
    public static string ClassifyFallbackLicenseSeatReservation(double score)
    {
        if (score < 10.9)
        {
            return "low";
        }
        if (score < 38.0)
        {
            return "medium";
        }
        return "watch";
    }

    /// <summary>Counts how many amounts fit in a budget of 94710.0, in order.</summary>
    public static int FallbackLicenseSeatReservationBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 94710.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
