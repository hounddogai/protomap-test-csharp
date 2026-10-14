namespace Acme.Workforce.Payrollops.PayRun.RecordItem;

/// <summary>Functions of pay run record policy.</summary>
public static class PayRunRecordPolicy
{
    public static int PayRunRecordSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 3600;
    }

    /// <summary>Counts how many amounts fit in a budget of 53607.0, in order.</summary>
    public static int PayRunRecordBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 53607.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a pay run record code in its canonical PRR- form.</summary>
    public static string NormalizePayRunRecordCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PRR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PRR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
