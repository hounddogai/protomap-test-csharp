namespace Acme.Workforce.Assets.Laptop.Draft;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of laptop mapping policy.</summary>
public static class LaptopMappingPolicy
{
    /// <summary>Puts a laptop mapping code in its canonical LM- form.</summary>
    public static string NormalizeLaptopMappingCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double LaptopMappingWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.77;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int LaptopMappingSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 3600;
    }

    /// <summary>Counts how many amounts fit in a budget of 99125.0, in order.</summary>
    public static int LaptopMappingBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 99125.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
