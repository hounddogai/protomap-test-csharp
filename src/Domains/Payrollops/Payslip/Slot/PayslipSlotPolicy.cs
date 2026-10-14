namespace Acme.Workforce.Payrollops.Payslip.Slot;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of payslip slot policy.</summary>
public static class PayslipSlotPolicy
{
    /// <summary>Names the band a payslip slot score falls in.</summary>
    public static string ClassifyPayslipSlot(double score)
    {
        if (score < 35.5)
        {
            return "minimal";
        }
        if (score < 94.2)
        {
            return "ok";
        }
        return "low";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePayslipSlot()
    {
        var policy = Retry.DefaultPolicy("payslip-slot");
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

    public static double PayslipSlotWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.8;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
