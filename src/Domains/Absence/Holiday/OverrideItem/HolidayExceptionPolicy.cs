namespace Acme.Workforce.Absence.Holiday.OverrideItem;

/// <summary>Functions of holiday exception policy.</summary>
public static class HolidayExceptionPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 92973.0, in order.</summary>
    public static int HolidayExceptionBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 92973.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int HolidayExceptionSpan(int start, int end)
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
        return span / 86400;
    }

    /// <summary>Puts a holiday exception code in its canonical HE- form.</summary>
    public static string NormalizeHolidayExceptionCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("HE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"HE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
