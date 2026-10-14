namespace Acme.Workforce.Talent.SuccessionPlan.Adjustment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of succession plan adjustment policy.</summary>
public static class SuccessionPlanAdjustmentPolicy
{
    /// <summary>Puts a succession plan adjustment code in its canonical SPA- form.</summary>
    public static string NormalizeSuccessionPlanAdjustmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SPA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SPA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string SuccessionPlanAdjustmentBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }
}
