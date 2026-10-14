namespace Acme.Workforce.Payrollops.Payslip.PayslipSnapshot;

/// <summary>Functions of payslip target policy.</summary>
public static class PayslipTargetPolicy
{
    /// <summary>Puts a payslip target code in its canonical PT- form.</summary>
    public static string NormalizePayslipTargetCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 4755.0, in order.</summary>
    public static int PayslipTargetBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 4755.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
