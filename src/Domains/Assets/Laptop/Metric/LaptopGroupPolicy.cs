namespace Acme.Workforce.Assets.Laptop.Metric;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of laptop group policy.</summary>
public static class LaptopGroupPolicy
{
    /// <summary>Puts a laptop group code in its canonical LG- form.</summary>
    public static string NormalizeLaptopGroupCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LG-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LG-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string LaptopGroupBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    public static int LaptopGroupSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Counts how many amounts fit in a budget of 80509.0, in order.</summary>
    public static int LaptopGroupBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 80509.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
