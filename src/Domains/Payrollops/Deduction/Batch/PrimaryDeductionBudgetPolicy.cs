namespace Acme.Workforce.Payrollops.Deduction.Batch;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of primary deduction budget policy.</summary>
public static class PrimaryDeductionBudgetPolicy
{
    /// <summary>Puts a primary deduction budget code in its canonical PDB- form.</summary>
    public static string NormalizePrimaryDeductionBudgetCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PDB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PDB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePrimaryDeductionBudget()
    {
        var policy = Retry.DefaultPolicy("primary-deduction-budget");
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

    public static string PrimaryDeductionBudgetBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    public static double PrimaryDeductionBudgetWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.46;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
