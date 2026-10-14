namespace Acme.Workforce.Payrollops.Deduction.Batch;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of deduction profile policy.</summary>
public static class DeductionProfilePolicy
{
    public static double DeductionProfileWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.71;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a deduction profile score falls in.</summary>
    public static string ClassifyDeductionProfile(double score)
    {
        if (score < 8.4)
        {
            return "elevated";
        }
        if (score < 74.0)
        {
            return "high";
        }
        if (score < 99.6)
        {
            return "ok";
        }
        return "watch";
    }

    public static int DeductionProfileSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 3600;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeDeductionProfile()
    {
        var policy = Retry.DefaultPolicy("deduction-profile");
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
}
