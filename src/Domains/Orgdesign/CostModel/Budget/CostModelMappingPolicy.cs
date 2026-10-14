namespace Acme.Workforce.Orgdesign.CostModel.Budget;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of cost model mapping policy.</summary>
public static class CostModelMappingPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 26678.0, in order.</summary>
    public static int CostModelMappingBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 26678.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a cost model mapping score falls in.</summary>
    public static string ClassifyCostModelMapping(double score)
    {
        if (score < 82.9)
        {
            return "critical";
        }
        if (score < 97.9)
        {
            return "low";
        }
        return "high";
    }

    public static List<string> MergeCostModelMappingLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a cost model mapping code in its canonical CMM- form.</summary>
    public static string NormalizeCostModelMappingCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CMM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CMM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
