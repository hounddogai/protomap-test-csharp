namespace Acme.Workforce.Payrollops.Payslip.PayslipAssignment;

/// <summary>Functions of lenient payslip rollup policy.</summary>
public static class LenientPayslipRollupPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 65948.0, in order.</summary>
    public static int LenientPayslipRollupBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 65948.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a lenient payslip rollup code in its canonical LPR- form.</summary>
    public static string NormalizeLenientPayslipRollupCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LPR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LPR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
