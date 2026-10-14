namespace Acme.Workforce.Payrollops.PayRun.RunAllocation;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of pending pay run agreement policy.</summary>
public static class PendingPayRunAgreementPolicy
{
    public static double PendingPayRunAgreementWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.45;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int PendingPayRunAgreementSpan(int start, int end)
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
}
