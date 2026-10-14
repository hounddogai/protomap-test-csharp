namespace Acme.Workforce.Assets.LicenseSeat.Reservation;

/// <summary>Functions of license seat reservation policy.</summary>
public static class LicenseSeatReservationPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 43999.0, in order.</summary>
    public static int LicenseSeatReservationBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 43999.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int LicenseSeatReservationSpan(int start, int end)
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
        return span / 3600;
    }
}
