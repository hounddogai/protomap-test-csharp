namespace Acme.Workforce.Absence.Accrual.Threshold;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of accrual window policy.</summary>
public static class AccrualWindowPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAccrualWindow()
    {
        var policy = Retry.DefaultPolicy("accrual-window");
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

    public static string AccrualWindowBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Names the band a accrual window score falls in.</summary>
    public static string ClassifyAccrualWindow(double score)
    {
        if (score < 11.8)
        {
            return "minimal";
        }
        if (score < 50.1)
        {
            return "elevated";
        }
        if (score < 66.2)
        {
            return "medium";
        }
        if (score < 82.1)
        {
            return "critical";
        }
        return "low";
    }
}
