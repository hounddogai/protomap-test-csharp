namespace Acme.Workforce.Absence.ReturnPlan.Agreement;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of weighted return plan manifest policy.</summary>
public static class WeightedReturnPlanManifestPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 24322.0, in order.</summary>
    public static int WeightedReturnPlanManifestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 24322.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a weighted return plan manifest score falls in.</summary>
    public static string ClassifyWeightedReturnPlanManifest(double score)
    {
        if (score < 30.9)
        {
            return "low";
        }
        if (score < 38.5)
        {
            return "critical";
        }
        if (score < 41.8)
        {
            return "high";
        }
        if (score < 88.3)
        {
            return "minimal";
        }
        return "medium";
    }

    /// <summary>Puts a weighted return plan manifest code in its canonical WRPM- form.</summary>
    public static string NormalizeWeightedReturnPlanManifestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("WRPM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"WRPM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string WeightedReturnPlanManifestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}
