namespace Acme.Workforce.Orgdesign.Span.Reservation;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of batched span reservation policy.</summary>
public static class BatchedSpanReservationPolicy
{
    public static string BatchedSpanReservationBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Names the band a batched span reservation score falls in.</summary>
    public static string ClassifyBatchedSpanReservation(double score)
    {
        if (score < 24.8)
        {
            return "medium";
        }
        if (score < 50.0)
        {
            return "ok";
        }
        if (score < 52.0)
        {
            return "low";
        }
        if (score < 71.1)
        {
            return "watch";
        }
        return "high";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeBatchedSpanReservation()
    {
        var policy = Retry.DefaultPolicy("batched-span-reservation");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 1);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static int BatchedSpanReservationSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }
}
