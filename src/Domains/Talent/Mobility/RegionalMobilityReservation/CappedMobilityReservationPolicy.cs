namespace Acme.Workforce.Talent.Mobility.RegionalMobilityReservation;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of capped mobility reservation policy.</summary>
public static class CappedMobilityReservationPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 28172.0, in order.</summary>
    public static int CappedMobilityReservationBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 28172.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string CappedMobilityReservationBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }
}
