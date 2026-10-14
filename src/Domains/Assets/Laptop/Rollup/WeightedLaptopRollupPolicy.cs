namespace Acme.Workforce.Assets.Laptop.Rollup;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of weighted laptop rollup policy.</summary>
public static class WeightedLaptopRollupPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 64740.0, in order.</summary>
    public static int WeightedLaptopRollupBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 64740.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeWeightedLaptopRollup()
    {
        var policy = Retry.DefaultPolicy("weighted-laptop-rollup");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 5);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Puts a weighted laptop rollup code in its canonical WLR- form.</summary>
    public static string NormalizeWeightedLaptopRollupCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("WLR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"WLR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int WeightedLaptopRollupSpan(int start, int end)
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
        return span / 3600;
    }
}
