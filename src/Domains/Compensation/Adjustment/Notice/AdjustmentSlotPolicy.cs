namespace Acme.Workforce.Compensation.Adjustment.Notice;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of adjustment slot policy.</summary>
public static class AdjustmentSlotPolicy
{
    public static double AdjustmentSlotWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.11;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int AdjustmentSlotSpan(int start, int end)
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
        return span / 86400;
    }

    /// <summary>Counts how many amounts fit in a budget of 75927.0, in order.</summary>
    public static int AdjustmentSlotBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 75927.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a adjustment slot code in its canonical AS- form.</summary>
    public static string NormalizeAdjustmentSlotCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
