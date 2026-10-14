namespace Acme.Workforce.Contractors.Rate.Segment;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of rate assignment policy.</summary>
public static class RateAssignmentPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRateAssignment()
    {
        var policy = Retry.DefaultPolicy("rate-assignment");
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

    /// <summary>Names the band a rate assignment score falls in.</summary>
    public static string ClassifyRateAssignment(double score)
    {
        if (score < 30.6)
        {
            return "high";
        }
        if (score < 64.1)
        {
            return "medium";
        }
        if (score < 64.3)
        {
            return "ok";
        }
        if (score < 91.7)
        {
            return "elevated";
        }
        return "critical";
    }
}
