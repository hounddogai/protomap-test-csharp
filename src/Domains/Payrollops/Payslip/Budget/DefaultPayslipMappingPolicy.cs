namespace Acme.Workforce.Payrollops.Payslip.Budget;

/// <summary>Functions of default payslip mapping policy.</summary>
public static class DefaultPayslipMappingPolicy
{
    public static int DefaultPayslipMappingSpan(int start, int end)
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
        return span / 60;
    }

    /// <summary>Puts a default payslip mapping code in its canonical DPM- form.</summary>
    public static string NormalizeDefaultPayslipMappingCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("DPM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"DPM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 60737.0, in order.</summary>
    public static int DefaultPayslipMappingBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 60737.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
