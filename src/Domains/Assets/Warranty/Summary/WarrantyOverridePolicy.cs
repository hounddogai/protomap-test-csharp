namespace Acme.Workforce.Assets.Warranty.Summary;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of warranty override policy.</summary>
public static class WarrantyOverridePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 30959.0, in order.</summary>
    public static int WarrantyOverrideBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 30959.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double WarrantyOverrideWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.41;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string WarrantyOverrideBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Puts a warranty override code in its canonical WO- form.</summary>
    public static string NormalizeWarrantyOverrideCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("WO-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"WO-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
