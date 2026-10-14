namespace Acme.Workforce.Orgdesign.Headcount.Rollup;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of legacy headcount assignment policy.</summary>
public static class LegacyHeadcountAssignmentPolicy
{
    /// <summary>Puts a legacy headcount assignment code in its canonical LHA- form.</summary>
    public static string NormalizeLegacyHeadcountAssignmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LHA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LHA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double LegacyHeadcountAssignmentWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.1;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a legacy headcount assignment score falls in.</summary>
    public static string ClassifyLegacyHeadcountAssignment(double score)
    {
        if (score < 8.9)
        {
            return "medium";
        }
        if (score < 32.1)
        {
            return "minimal";
        }
        if (score < 36.9)
        {
            return "low";
        }
        if (score < 44.2)
        {
            return "ok";
        }
        return "high";
    }

    /// <summary>Counts how many amounts fit in a budget of 79436.0, in order.</summary>
    public static int LegacyHeadcountAssignmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 79436.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
