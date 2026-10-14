namespace Acme.Workforce.Orgdesign.Headcount.LegacyHeadcountRecord;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of primary headcount agreement policy.</summary>
public static class PrimaryHeadcountAgreementPolicy
{
    public static string PrimaryHeadcountAgreementBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Puts a primary headcount agreement code in its canonical PHA- form.</summary>
    public static string NormalizePrimaryHeadcountAgreementCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PHA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PHA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double PrimaryHeadcountAgreementWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.16;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 81833.0, in order.</summary>
    public static int PrimaryHeadcountAgreementBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 81833.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
