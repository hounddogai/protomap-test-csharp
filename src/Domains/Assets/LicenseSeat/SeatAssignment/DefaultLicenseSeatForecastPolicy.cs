namespace Acme.Workforce.Assets.LicenseSeat.SeatAssignment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of default license seat forecast policy.</summary>
public static class DefaultLicenseSeatForecastPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 14532.0, in order.</summary>
    public static int DefaultLicenseSeatForecastBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 14532.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string DefaultLicenseSeatForecastBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}
