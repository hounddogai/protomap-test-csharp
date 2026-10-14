namespace Acme.Workforce.Payrollops.Deduction.DeductionBaseline;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of global deduction baseline policy.</summary>
public static class GlobalDeductionBaselinePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 87175.0, in order.</summary>
    public static int GlobalDeductionBaselineBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 87175.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string GlobalDeductionBaselineBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Puts a global deduction baseline code in its canonical GDB- form.</summary>
    public static string NormalizeGlobalDeductionBaselineCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("GDB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"GDB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
