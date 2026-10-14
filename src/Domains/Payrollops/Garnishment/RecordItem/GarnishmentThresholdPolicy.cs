namespace Acme.Workforce.Payrollops.Garnishment.RecordItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of garnishment threshold policy.</summary>
public static class GarnishmentThresholdPolicy
{
    /// <summary>Puts a garnishment threshold code in its canonical GT- form.</summary>
    public static string NormalizeGarnishmentThresholdCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("GT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"GT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 52927.0, in order.</summary>
    public static int GarnishmentThresholdBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 52927.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string GarnishmentThresholdBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    public static double GarnishmentThresholdWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.95;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
