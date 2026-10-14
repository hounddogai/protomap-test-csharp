namespace Acme.Workforce.Orgdesign.CostModel.Summary;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of cost model summary policy.</summary>
public static class CostModelSummaryPolicy
{
    public static double CostModelSummaryWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.93;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 72492.0, in order.</summary>
    public static int CostModelSummaryBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 72492.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a cost model summary code in its canonical CMS- form.</summary>
    public static string NormalizeCostModelSummaryCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CMS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CMS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string CostModelSummaryBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }
}
