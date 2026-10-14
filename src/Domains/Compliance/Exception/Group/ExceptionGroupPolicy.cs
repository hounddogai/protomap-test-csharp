namespace Acme.Workforce.Compliance.Exception.Group;

/// <summary>Functions of exception group policy.</summary>
public static class ExceptionGroupPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 4510.0, in order.</summary>
    public static int ExceptionGroupBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 4510.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a exception group code in its canonical EG- form.</summary>
    public static string NormalizeExceptionGroupCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("EG-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"EG-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int ExceptionGroupSpan(int start, int end)
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
}
