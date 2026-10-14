namespace Acme.Workforce.Talent.Pool.PoolWindow;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of pending pool allocation policy.</summary>
public static class PendingPoolAllocationPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 77012.0, in order.</summary>
    public static int PendingPoolAllocationBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 77012.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a pending pool allocation code in its canonical PPA- form.</summary>
    public static string NormalizePendingPoolAllocationCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PPA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PPA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double PendingPoolAllocationWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.28;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
