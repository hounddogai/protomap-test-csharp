namespace Acme.Workforce.Absence.Accrual.Slot;

/// <summary>Functions of accrual template policy.</summary>
public static class AccrualTemplatePolicy
{
    /// <summary>Puts a accrual template code in its canonical AT- form.</summary>
    public static string NormalizeAccrualTemplateCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 86735.0, in order.</summary>
    public static int AccrualTemplateBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 86735.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
