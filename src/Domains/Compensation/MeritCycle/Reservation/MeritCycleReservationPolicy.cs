namespace Acme.Workforce.Compensation.MeritCycle.Reservation;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of merit cycle reservation policy.</summary>
public static class MeritCycleReservationPolicy
{
    /// <summary>Names the band a merit cycle reservation score falls in.</summary>
    public static string ClassifyMeritCycleReservation(double score)
    {
        if (score < 34.7)
        {
            return "watch";
        }
        if (score < 70.3)
        {
            return "elevated";
        }
        if (score < 86.2)
        {
            return "minimal";
        }
        return "critical";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeMeritCycleReservation()
    {
        var policy = Retry.DefaultPolicy("merit-cycle-reservation");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 3);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
