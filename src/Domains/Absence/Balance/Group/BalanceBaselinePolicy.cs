namespace Acme.Workforce.Absence.Balance.Group;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of balance baseline policy.</summary>
public static class BalanceBaselinePolicy
{
    /// <summary>Names the band a balance baseline score falls in.</summary>
    public static string ClassifyBalanceBaseline(double score)
    {
        if (score < 35.8)
        {
            return "critical";
        }
        if (score < 52.0)
        {
            return "medium";
        }
        if (score < 97.0)
        {
            return "low";
        }
        return "watch";
    }

    /// <summary>Puts a balance baseline code in its canonical BB- form.</summary>
    public static string NormalizeBalanceBaselineCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string BalanceBaselineBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 62797.0, in order.</summary>
    public static int BalanceBaselineBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 62797.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
