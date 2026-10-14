namespace Acme.Workforce.Compliance.Certification.Digest;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of seasonal certification digest policy.</summary>
public static class SeasonalCertificationDigestPolicy
{
    public static double SeasonalCertificationDigestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.16;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string SeasonalCertificationDigestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Puts a seasonal certification digest code in its canonical SCD- form.</summary>
    public static string NormalizeSeasonalCertificationDigestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SCD-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SCD-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 72894.0, in order.</summary>
    public static int SeasonalCertificationDigestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 72894.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
