namespace Acme.Workforce.Assets.Warranty.Reservation;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of warranty reservation policy.</summary>
public static class WarrantyReservationPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeWarrantyReservation()
    {
        var policy = Retry.DefaultPolicy("warranty-reservation");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 5);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Names the band a warranty reservation score falls in.</summary>
    public static string ClassifyWarrantyReservation(double score)
    {
        if (score < 10.1)
        {
            return "critical";
        }
        if (score < 72.3)
        {
            return "minimal";
        }
        if (score < 94.7)
        {
            return "elevated";
        }
        return "high";
    }

    public static string WarrantyReservationBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    public static double WarrantyReservationWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.6;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
