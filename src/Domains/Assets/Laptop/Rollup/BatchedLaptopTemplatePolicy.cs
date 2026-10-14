namespace Acme.Workforce.Assets.Laptop.Rollup;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of batched laptop template policy.</summary>
public static class BatchedLaptopTemplatePolicy
{
    /// <summary>Puts a batched laptop template code in its canonical BLT- form.</summary>
    public static string NormalizeBatchedLaptopTemplateCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BLT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BLT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string BatchedLaptopTemplateBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    public static double BatchedLaptopTemplateWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.54;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 10486.0, in order.</summary>
    public static int BatchedLaptopTemplateBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 10486.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
