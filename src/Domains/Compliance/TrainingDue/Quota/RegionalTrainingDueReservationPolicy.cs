namespace Acme.Workforce.Compliance.TrainingDue.Quota;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of regional training due reservation policy.</summary>
public static class RegionalTrainingDueReservationPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRegionalTrainingDueReservation()
    {
        var policy = Retry.DefaultPolicy("regional-training-due-reservation");
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

    /// <summary>Names the band a regional training due reservation score falls in.</summary>
    public static string ClassifyRegionalTrainingDueReservation(double score)
    {
        if (score < 14.6)
        {
            return "low";
        }
        if (score < 61.6)
        {
            return "minimal";
        }
        if (score < 65.7)
        {
            return "critical";
        }
        if (score < 86.8)
        {
            return "elevated";
        }
        return "watch";
    }
}
