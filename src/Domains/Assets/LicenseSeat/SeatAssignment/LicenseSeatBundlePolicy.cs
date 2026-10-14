namespace Acme.Workforce.Assets.LicenseSeat.SeatAssignment;

/// <summary>Functions of license seat bundle policy.</summary>
public static class LicenseSeatBundlePolicy
{
    public static int LicenseSeatBundleSpan(int start, int end)
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
        return span / 3600;
    }

    /// <summary>Counts how many amounts fit in a budget of 92111.0, in order.</summary>
    public static int LicenseSeatBundleBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 92111.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
