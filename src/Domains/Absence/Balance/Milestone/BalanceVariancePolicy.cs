namespace Acme.Workforce.Absence.Balance.Milestone;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of balance variance policy.</summary>
public static class BalanceVariancePolicy
{
    public static double BalanceVarianceWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.44;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string BalanceVarianceBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 7585.0, in order.</summary>
    public static int BalanceVarianceBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 7585.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a balance variance code in its canonical BV- form.</summary>
    public static string NormalizeBalanceVarianceCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BV-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BV-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
