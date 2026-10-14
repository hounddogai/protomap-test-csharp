namespace Acme.Workforce.Compensation.Adjustment.AdjustmentAgreement;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of pending adjustment agreement policy.</summary>
public static class PendingAdjustmentAgreementPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePendingAdjustmentAgreement()
    {
        var policy = Retry.DefaultPolicy("pending-adjustment-agreement");
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

    public static double PendingAdjustmentAgreementWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.05;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string PendingAdjustmentAgreementBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Names the band a pending adjustment agreement score falls in.</summary>
    public static string ClassifyPendingAdjustmentAgreement(double score)
    {
        if (score < 16.7)
        {
            return "critical";
        }
        if (score < 32.6)
        {
            return "minimal";
        }
        return "low";
    }
}
