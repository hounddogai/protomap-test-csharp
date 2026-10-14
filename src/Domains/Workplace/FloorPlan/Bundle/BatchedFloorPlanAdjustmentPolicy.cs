namespace Acme.Workforce.Workplace.FloorPlan.Bundle;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of batched floor plan adjustment policy.</summary>
public static class BatchedFloorPlanAdjustmentPolicy
{
    public static double BatchedFloorPlanAdjustmentWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.63;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 93776.0, in order.</summary>
    public static int BatchedFloorPlanAdjustmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 93776.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a batched floor plan adjustment code in its canonical BFPA- form.</summary>
    public static string NormalizeBatchedFloorPlanAdjustmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BFPA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BFPA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
