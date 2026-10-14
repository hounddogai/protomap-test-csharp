namespace Acme.Workforce.Absence.Holiday.Metric;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of holiday agreement policy.</summary>
public static class HolidayAgreementPolicy
{
    public static int HolidayAgreementSpan(int start, int end)
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
        return span / 3600;
    }

    /// <summary>Counts how many amounts fit in a budget of 81719.0, in order.</summary>
    public static int HolidayAgreementBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 81719.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a holiday agreement code in its canonical HA- form.</summary>
    public static string NormalizeHolidayAgreementCode(string code)
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

    public static string HolidayAgreementBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
