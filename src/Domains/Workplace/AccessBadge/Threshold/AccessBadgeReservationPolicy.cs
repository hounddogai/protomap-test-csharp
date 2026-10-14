namespace Acme.Workforce.Workplace.AccessBadge.Threshold;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of access badge reservation policy.</summary>
public static class AccessBadgeReservationPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 61479.0, in order.</summary>
    public static int AccessBadgeReservationBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 61479.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string AccessBadgeReservationBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    public static int AccessBadgeReservationSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 86400;
    }
}
