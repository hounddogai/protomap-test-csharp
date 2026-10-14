namespace Acme.Workforce.Orgdesign.CostModel.Entry;

/// <summary>Functions of cost model entry policy.</summary>
public static class CostModelEntryPolicy
{
    /// <summary>Names the band a cost model entry score falls in.</summary>
    public static string ClassifyCostModelEntry(double score)
    {
        if (score < 12.6)
        {
            return "minimal";
        }
        if (score < 60.7)
        {
            return "critical";
        }
        if (score < 89.1)
        {
            return "ok";
        }
        return "elevated";
    }

    /// <summary>Puts a cost model entry code in its canonical CME- form.</summary>
    public static string NormalizeCostModelEntryCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CME-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CME-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 87055.0, in order.</summary>
    public static int CostModelEntryBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 87055.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
