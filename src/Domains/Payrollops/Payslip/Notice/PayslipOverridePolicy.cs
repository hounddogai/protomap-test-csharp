namespace Acme.Workforce.Payrollops.Payslip.Notice;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of payslip override policy.</summary>
public static class PayslipOverridePolicy
{
    public static List<string> MergePayslipOverrideLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePayslipOverride()
    {
        var policy = Retry.DefaultPolicy("payslip-override");
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

    /// <summary>Names the band a payslip override score falls in.</summary>
    public static string ClassifyPayslipOverride(double score)
    {
        if (score < 38.1)
        {
            return "medium";
        }
        if (score < 54.3)
        {
            return "high";
        }
        if (score < 65.5)
        {
            return "watch";
        }
        return "elevated";
    }
}
