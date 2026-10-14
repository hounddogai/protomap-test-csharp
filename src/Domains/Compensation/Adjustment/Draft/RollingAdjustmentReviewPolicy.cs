namespace Acme.Workforce.Compensation.Adjustment.Draft;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of rolling adjustment review policy.</summary>
public static class RollingAdjustmentReviewPolicy
{
    /// <summary>Names the band a rolling adjustment review score falls in.</summary>
    public static string ClassifyRollingAdjustmentReview(double score)
    {
        if (score < 19.9)
        {
            return "medium";
        }
        if (score < 67.4)
        {
            return "watch";
        }
        return "critical";
    }

    /// <summary>Counts how many amounts fit in a budget of 42841.0, in order.</summary>
    public static int RollingAdjustmentReviewBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 42841.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRollingAdjustmentReview()
    {
        var policy = Retry.DefaultPolicy("rolling-adjustment-review");
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

    public static double RollingAdjustmentReviewWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.44;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
