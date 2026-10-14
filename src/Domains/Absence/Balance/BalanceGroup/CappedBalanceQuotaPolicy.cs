namespace Acme.Workforce.Absence.Balance.BalanceGroup;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of capped balance quota policy.</summary>
public static class CappedBalanceQuotaPolicy
{
    /// <summary>Puts a capped balance quota code in its canonical CBQ- form.</summary>
    public static string NormalizeCappedBalanceQuotaCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CBQ-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CBQ-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int CappedBalanceQuotaSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 60;
    }

    public static double CappedBalanceQuotaWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.98;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 25994.0, in order.</summary>
    public static int CappedBalanceQuotaBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 25994.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
