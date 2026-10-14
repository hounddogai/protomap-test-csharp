namespace Acme.Workforce.Payrollops.Payslip.PayslipAssignment;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of rolling payslip assignment policy.</summary>
public static class RollingPayslipAssignmentPolicy
{
    public static string RollingPayslipAssignmentBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Names the band a rolling payslip assignment score falls in.</summary>
    public static string ClassifyRollingPayslipAssignment(double score)
    {
        if (score < 74.9)
        {
            return "elevated";
        }
        if (score < 91.9)
        {
            return "high";
        }
        return "medium";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRollingPayslipAssignment()
    {
        var policy = Retry.DefaultPolicy("rolling-payslip-assignment");
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

    public static int RollingPayslipAssignmentSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 3600;
    }
}
