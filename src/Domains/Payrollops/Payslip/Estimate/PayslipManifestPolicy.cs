namespace Acme.Workforce.Payrollops.Payslip.Estimate;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of payslip manifest policy.</summary>
public static class PayslipManifestPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePayslipManifest()
    {
        var policy = Retry.DefaultPolicy("payslip-manifest");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 1);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Names the band a payslip manifest score falls in.</summary>
    public static string ClassifyPayslipManifest(double score)
    {
        if (score < 18.0)
        {
            return "watch";
        }
        if (score < 26.5)
        {
            return "ok";
        }
        if (score < 49.2)
        {
            return "minimal";
        }
        if (score < 50.8)
        {
            return "critical";
        }
        return "medium";
    }

    public static int PayslipManifestSpan(int start, int end)
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
        return span / 3600;
    }

    public static double PayslipManifestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.3;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
