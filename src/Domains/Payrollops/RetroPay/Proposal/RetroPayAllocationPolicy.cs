namespace Acme.Workforce.Payrollops.RetroPay.Proposal;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of retro pay allocation policy.</summary>
public static class RetroPayAllocationPolicy
{
    public static double RetroPayAllocationWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.48;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int RetroPayAllocationSpan(int start, int end)
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
        return span / 86400;
    }
}
