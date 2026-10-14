namespace Acme.Workforce.Workplace.Occupancy.Reservation;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of lenient occupancy estimate policy.</summary>
public static class LenientOccupancyEstimatePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 97926.0, in order.</summary>
    public static int LenientOccupancyEstimateBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 97926.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string LenientOccupancyEstimateBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }
}
