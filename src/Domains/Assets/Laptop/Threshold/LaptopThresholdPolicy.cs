namespace Acme.Workforce.Assets.Laptop.Threshold;

/// <summary>Functions of laptop threshold policy.</summary>
public static class LaptopThresholdPolicy
{
    public static int LaptopThresholdSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }

    /// <summary>Puts a laptop threshold code in its canonical LT- form.</summary>
    public static string NormalizeLaptopThresholdCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 42225.0, in order.</summary>
    public static int LaptopThresholdBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 42225.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
