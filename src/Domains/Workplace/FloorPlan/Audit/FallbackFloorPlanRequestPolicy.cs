namespace Acme.Workforce.Workplace.FloorPlan.Audit;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of fallback floor plan request policy.</summary>
public static class FallbackFloorPlanRequestPolicy
{
    /// <summary>Puts a fallback floor plan request code in its canonical FFPR- form.</summary>
    public static string NormalizeFallbackFloorPlanRequestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FFPR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FFPR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double FallbackFloorPlanRequestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.78;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 96308.0, in order.</summary>
    public static int FallbackFloorPlanRequestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 96308.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
