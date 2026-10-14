namespace Acme.Workforce.Workplace.Occupancy.OccupancyPlan;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of lenient occupancy plan policy.</summary>
public static class LenientOccupancyPlanPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLenientOccupancyPlan()
    {
        var policy = Retry.DefaultPolicy("lenient-occupancy-plan");
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

    /// <summary>Puts a lenient occupancy plan code in its canonical LOP- form.</summary>
    public static string NormalizeLenientOccupancyPlanCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LOP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LOP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
