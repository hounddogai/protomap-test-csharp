namespace Acme.Workforce.Compensation.MeritCycle.CycleReview;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of primary merit cycle slot policy.</summary>
public static class PrimaryMeritCycleSlotPolicy
{
    public static string PrimaryMeritCycleSlotBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Puts a primary merit cycle slot code in its canonical PMCS- form.</summary>
    public static string NormalizePrimaryMeritCycleSlotCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PMCS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PMCS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePrimaryMeritCycleSlot()
    {
        var policy = Retry.DefaultPolicy("primary-merit-cycle-slot");
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

    /// <summary>Names the band a primary merit cycle slot score falls in.</summary>
    public static string ClassifyPrimaryMeritCycleSlot(double score)
    {
        if (score < 50.3)
        {
            return "elevated";
        }
        if (score < 54.9)
        {
            return "watch";
        }
        return "critical";
    }
}
