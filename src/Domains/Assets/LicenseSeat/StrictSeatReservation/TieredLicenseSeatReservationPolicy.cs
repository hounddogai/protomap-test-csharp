namespace Acme.Workforce.Assets.LicenseSeat.StrictSeatReservation;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of tiered license seat reservation policy.</summary>
public static class TieredLicenseSeatReservationPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeTieredLicenseSeatReservation()
    {
        var policy = Retry.DefaultPolicy("tiered-license-seat-reservation");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 2);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static string TieredLicenseSeatReservationBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 28189.0, in order.</summary>
    public static int TieredLicenseSeatReservationBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 28189.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
