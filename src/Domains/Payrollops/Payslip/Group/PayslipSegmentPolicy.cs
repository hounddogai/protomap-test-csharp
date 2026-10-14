namespace Acme.Workforce.Payrollops.Payslip.Group;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of payslip segment policy.</summary>
public static class PayslipSegmentPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePayslipSegment()
    {
        var policy = Retry.DefaultPolicy("payslip-segment");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 3);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Names the band a payslip segment score falls in.</summary>
    public static string ClassifyPayslipSegment(double score)
    {
        if (score < 8.2)
        {
            return "ok";
        }
        if (score < 90.2)
        {
            return "watch";
        }
        return "medium";
    }
}
