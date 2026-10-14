namespace Acme.Workforce.Payrollops.PayRun.Budget;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of scheduled pay run budget policy.</summary>
public static class ScheduledPayRunBudgetPolicy
{
    public static double ScheduledPayRunBudgetWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.41;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string ScheduledPayRunBudgetBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
