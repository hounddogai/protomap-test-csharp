namespace Acme.Workforce.Absence.Balance.DefaultBalanceGroup;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of lenient balance group policy.</summary>
public static class LenientBalanceGroupPolicy
{
    public static double LenientBalanceGroupWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.74;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 69383.0, in order.</summary>
    public static int LenientBalanceGroupBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 69383.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string LenientBalanceGroupBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Puts a lenient balance group code in its canonical LBG- form.</summary>
    public static string NormalizeLenientBalanceGroupCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LBG-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LBG-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
