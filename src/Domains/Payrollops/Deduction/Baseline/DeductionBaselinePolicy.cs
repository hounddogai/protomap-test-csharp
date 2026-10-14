namespace Acme.Workforce.Payrollops.Deduction.Baseline;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of deduction baseline policy.</summary>
public static class DeductionBaselinePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 46225.0, in order.</summary>
    public static int DeductionBaselineBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 46225.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double DeductionBaselineWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.68;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a deduction baseline score falls in.</summary>
    public static string ClassifyDeductionBaseline(double score)
    {
        if (score < 83.9)
        {
            return "watch";
        }
        if (score < 86.0)
        {
            return "ok";
        }
        if (score < 90.0)
        {
            return "minimal";
        }
        return "high";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeDeductionBaseline()
    {
        var policy = Retry.DefaultPolicy("deduction-baseline");
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
}
