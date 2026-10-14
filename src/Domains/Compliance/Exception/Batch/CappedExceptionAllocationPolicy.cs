namespace Acme.Workforce.Compliance.Exception.Batch;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of capped exception allocation policy.</summary>
public static class CappedExceptionAllocationPolicy
{
    public static double CappedExceptionAllocationWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.96;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeCappedExceptionAllocation()
    {
        var policy = Retry.DefaultPolicy("capped-exception-allocation");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 4);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Names the band a capped exception allocation score falls in.</summary>
    public static string ClassifyCappedExceptionAllocation(double score)
    {
        if (score < 46.3)
        {
            return "high";
        }
        if (score < 71.0)
        {
            return "elevated";
        }
        return "watch";
    }
}
