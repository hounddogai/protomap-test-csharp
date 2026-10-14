namespace Acme.Workforce.Compensation.EquityGrant.GrantDigest;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of pending equity grant variance policy.</summary>
public static class PendingEquityGrantVariancePolicy
{
    /// <summary>Puts a pending equity grant variance code in its canonical PEGV- form.</summary>
    public static string NormalizePendingEquityGrantVarianceCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PEGV-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PEGV-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a pending equity grant variance score falls in.</summary>
    public static string ClassifyPendingEquityGrantVariance(double score)
    {
        if (score < 31.9)
        {
            return "high";
        }
        if (score < 33.0)
        {
            return "minimal";
        }
        if (score < 57.4)
        {
            return "elevated";
        }
        return "low";
    }

    public static double PendingEquityGrantVarianceWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.49;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 68825.0, in order.</summary>
    public static int PendingEquityGrantVarianceBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 68825.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
