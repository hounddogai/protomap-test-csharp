namespace Acme.Workforce.Orgdesign.Headcount.Proposal;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of adaptive headcount audit policy.</summary>
public static class AdaptiveHeadcountAuditPolicy
{
    /// <summary>Names the band a adaptive headcount audit score falls in.</summary>
    public static string ClassifyAdaptiveHeadcountAudit(double score)
    {
        if (score < 12.6)
        {
            return "ok";
        }
        if (score < 30.8)
        {
            return "high";
        }
        return "medium";
    }

    public static double AdaptiveHeadcountAuditWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.53;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a adaptive headcount audit code in its canonical AHA- form.</summary>
    public static string NormalizeAdaptiveHeadcountAuditCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AHA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AHA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 97886.0, in order.</summary>
    public static int AdaptiveHeadcountAuditBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 97886.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
