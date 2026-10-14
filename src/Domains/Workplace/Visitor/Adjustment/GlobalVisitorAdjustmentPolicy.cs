namespace Acme.Workforce.Workplace.Visitor.Adjustment;

/// <summary>Functions of global visitor adjustment policy.</summary>
public static class GlobalVisitorAdjustmentPolicy
{
    /// <summary>Puts a global visitor adjustment code in its canonical GVA- form.</summary>
    public static string NormalizeGlobalVisitorAdjustmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("GVA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"GVA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 13766.0, in order.</summary>
    public static int GlobalVisitorAdjustmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 13766.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
