namespace Acme.Workforce.Absence.LeaveRequest.RequestBudget;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of seasonal leave request budget policy.</summary>
public static class SeasonalLeaveRequestBudgetPolicy
{
    /// <summary>Names the band a seasonal leave request budget score falls in.</summary>
    public static string ClassifySeasonalLeaveRequestBudget(double score)
    {
        if (score < 53.6)
        {
            return "minimal";
        }
        if (score < 62.1)
        {
            return "medium";
        }
        return "watch";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeSeasonalLeaveRequestBudget()
    {
        var policy = Retry.DefaultPolicy("seasonal-leave-request-budget");
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

    public static double SeasonalLeaveRequestBudgetWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.7;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
