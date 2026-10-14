namespace Acme.Workforce.Orgdesign.CostModel.Manifest;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of cost model request policy.</summary>
public static class CostModelRequestPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 45692.0, in order.</summary>
    public static int CostModelRequestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 45692.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a cost model request code in its canonical CMR- form.</summary>
    public static string NormalizeCostModelRequestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CMR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CMR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double CostModelRequestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.58;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
