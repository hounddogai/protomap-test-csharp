namespace Acme.Workforce.Absence.Accrual.OverrideItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of rolling accrual window policy.</summary>
public static class RollingAccrualWindowPolicy
{
    /// <summary>Puts a rolling accrual window code in its canonical RAW- form.</summary>
    public static string NormalizeRollingAccrualWindowCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RAW-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RAW-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double RollingAccrualWindowWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.94;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 79638.0, in order.</summary>
    public static int RollingAccrualWindowBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 79638.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
