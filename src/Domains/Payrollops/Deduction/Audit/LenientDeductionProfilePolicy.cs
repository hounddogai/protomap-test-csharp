namespace Acme.Workforce.Payrollops.Deduction.Audit;

/// <summary>Functions of lenient deduction profile policy.</summary>
public static class LenientDeductionProfilePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 52725.0, in order.</summary>
    public static int LenientDeductionProfileBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 52725.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a lenient deduction profile code in its canonical LDP- form.</summary>
    public static string NormalizeLenientDeductionProfileCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LDP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LDP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
