namespace Acme.Workforce.Absence.Accrual.Bundle;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of tiered accrual score policy.</summary>
public static class TieredAccrualScorePolicy
{
    public static double TieredAccrualScoreWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.7;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 21313.0, in order.</summary>
    public static int TieredAccrualScoreBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 21313.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a tiered accrual score code in its canonical TAS- form.</summary>
    public static string NormalizeTieredAccrualScoreCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TAS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TAS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
