namespace Acme.Workforce.Compensation.EquityGrant.RecordItem;

/// <summary>Functions of equity grant record policy.</summary>
public static class EquityGrantRecordPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 16435.0, in order.</summary>
    public static int EquityGrantRecordBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 16435.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a equity grant record code in its canonical EGR- form.</summary>
    public static string NormalizeEquityGrantRecordCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("EGR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"EGR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
