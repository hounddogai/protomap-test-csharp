namespace Acme.Workforce.Compensation.Adjustment.Schedule;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of adjustment draft policy.</summary>
public static class AdjustmentDraftPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 45586.0, in order.</summary>
    public static int AdjustmentDraftBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 45586.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a adjustment draft code in its canonical AD- form.</summary>
    public static string NormalizeAdjustmentDraftCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AD-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AD-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double AdjustmentDraftWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.07;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
