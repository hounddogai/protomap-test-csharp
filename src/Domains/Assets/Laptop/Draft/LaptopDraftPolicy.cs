namespace Acme.Workforce.Assets.Laptop.Draft;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of laptop draft policy.</summary>
public static class LaptopDraftPolicy
{
    public static int LaptopDraftSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 3600;
    }

    /// <summary>Counts how many amounts fit in a budget of 328.0, in order.</summary>
    public static int LaptopDraftBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 328.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string LaptopDraftBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Puts a laptop draft code in its canonical LD- form.</summary>
    public static string NormalizeLaptopDraftCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LD-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LD-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
