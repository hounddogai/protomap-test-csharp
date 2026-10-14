namespace Acme.Workforce.Compliance.Certification.Notice;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of weighted certification batch policy.</summary>
public static class WeightedCertificationBatchPolicy
{
    public static int WeightedCertificationBatchSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 86400;
    }

    /// <summary>Puts a weighted certification batch code in its canonical WCB- form.</summary>
    public static string NormalizeWeightedCertificationBatchCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("WCB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"WCB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double WeightedCertificationBatchWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.08;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 14610.0, in order.</summary>
    public static int WeightedCertificationBatchBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 14610.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
