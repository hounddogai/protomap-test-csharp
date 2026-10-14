namespace Acme.Workforce.Assets.Refresh.Reservation;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of manual refresh reservation policy.</summary>
public static class ManualRefreshReservationPolicy
{
    public static string ManualRefreshReservationBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 76165.0, in order.</summary>
    public static int ManualRefreshReservationBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 76165.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
