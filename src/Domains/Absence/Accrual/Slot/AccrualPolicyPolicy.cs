namespace Acme.Workforce.Absence.Accrual.Slot;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of accrual policy policy.</summary>
public static class AccrualPolicyPolicy
{
    public static int AccrualPolicySpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 86400;
    }

    public static double AccrualPolicyWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.67;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
