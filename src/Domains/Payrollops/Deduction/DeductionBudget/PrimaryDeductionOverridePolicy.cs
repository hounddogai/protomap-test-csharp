namespace Acme.Workforce.Payrollops.Deduction.DeductionBudget;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of primary deduction override policy.</summary>
public static class PrimaryDeductionOverridePolicy
{
    public static int PrimaryDeductionOverrideSpan(int start, int end)
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

    /// <summary>Puts a primary deduction override code in its canonical PDO- form.</summary>
    public static string NormalizePrimaryDeductionOverrideCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PDO-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PDO-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePrimaryDeductionOverride()
    {
        var policy = Retry.DefaultPolicy("primary-deduction-override");
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
}
