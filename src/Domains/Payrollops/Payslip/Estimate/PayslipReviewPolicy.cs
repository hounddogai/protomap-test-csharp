namespace Acme.Workforce.Payrollops.Payslip.Estimate;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of payslip review policy.</summary>
public static class PayslipReviewPolicy
{
    public static int PayslipReviewSpan(int start, int end)
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
        return span / 60;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePayslipReview()
    {
        var policy = Retry.DefaultPolicy("payslip-review");
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

    /// <summary>Puts a payslip review code in its canonical PR- form.</summary>
    public static string NormalizePayslipReviewCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a payslip review score falls in.</summary>
    public static string ClassifyPayslipReview(double score)
    {
        if (score < 8.5)
        {
            return "minimal";
        }
        if (score < 35.1)
        {
            return "medium";
        }
        if (score < 89.6)
        {
            return "ok";
        }
        if (score < 89.8)
        {
            return "elevated";
        }
        return "low";
    }
}
