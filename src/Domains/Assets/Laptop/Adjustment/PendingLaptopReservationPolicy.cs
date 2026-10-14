namespace Acme.Workforce.Assets.Laptop.Adjustment;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of pending laptop reservation policy.</summary>
public static class PendingLaptopReservationPolicy
{
    public static int PendingLaptopReservationSpan(int start, int end)
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
        return span / 86400;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePendingLaptopReservation()
    {
        var policy = Retry.DefaultPolicy("pending-laptop-reservation");
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
