namespace Acme.Workforce.Payrollops.Deduction.Snapshot;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of rolling deduction entry policy.</summary>
public static class RollingDeductionEntryPolicy
{
    public static string RollingDeductionEntryBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    public static double RollingDeductionEntryWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.21;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
