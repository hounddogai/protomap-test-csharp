namespace Acme.Workforce.Orgdesign.Headcount.Assignment;

/// <summary>Functions of headcount audit policy.</summary>
public static class HeadcountAuditPolicy
{
    public static int HeadcountAuditSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 3600;
    }

    /// <summary>Puts a headcount audit code in its canonical HA- form.</summary>
    public static string NormalizeHeadcountAuditCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("HA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"HA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 9945.0, in order.</summary>
    public static int HeadcountAuditBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 9945.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
