namespace Acme.Workforce.Compensation.MeritCycle.Allocation;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of merit cycle allocation policy.</summary>
public static class MeritCycleAllocationPolicy
{
    public static List<string> MergeMeritCycleAllocationLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeMeritCycleAllocation()
    {
        var policy = Retry.DefaultPolicy("merit-cycle-allocation");
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

    public static string MeritCycleAllocationBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Names the band a merit cycle allocation score falls in.</summary>
    public static string ClassifyMeritCycleAllocation(double score)
    {
        if (score < 52.8)
        {
            return "critical";
        }
        if (score < 55.7)
        {
            return "medium";
        }
        if (score < 80.4)
        {
            return "elevated";
        }
        return "ok";
    }
}
