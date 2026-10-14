namespace Acme.Workforce.Assets.Laptop.Bundle;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of tiered laptop checkpoint policy.</summary>
public static class TieredLaptopCheckpointPolicy
{
    public static string TieredLaptopCheckpointBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 22327.0, in order.</summary>
    public static int TieredLaptopCheckpointBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 22327.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a tiered laptop checkpoint code in its canonical TLC- form.</summary>
    public static string NormalizeTieredLaptopCheckpointCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TLC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TLC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
