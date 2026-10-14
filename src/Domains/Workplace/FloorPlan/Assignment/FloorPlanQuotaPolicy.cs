namespace Acme.Workforce.Workplace.FloorPlan.Assignment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of floor plan quota policy.</summary>
public static class FloorPlanQuotaPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 71948.0, in order.</summary>
    public static int FloorPlanQuotaBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 71948.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static List<string> MergeFloorPlanQuotaLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double FloorPlanQuotaWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.02;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a floor plan quota code in its canonical FPQ- form.</summary>
    public static string NormalizeFloorPlanQuotaCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FPQ-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FPQ-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
