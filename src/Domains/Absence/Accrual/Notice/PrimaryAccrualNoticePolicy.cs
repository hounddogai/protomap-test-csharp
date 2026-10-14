namespace Acme.Workforce.Absence.Accrual.Notice;

/// <summary>Functions of primary accrual notice policy.</summary>
public static class PrimaryAccrualNoticePolicy
{
    /// <summary>Puts a primary accrual notice code in its canonical PAN- form.</summary>
    public static string NormalizePrimaryAccrualNoticeCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PAN-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PAN-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 17624.0, in order.</summary>
    public static int PrimaryAccrualNoticeBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 17624.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
