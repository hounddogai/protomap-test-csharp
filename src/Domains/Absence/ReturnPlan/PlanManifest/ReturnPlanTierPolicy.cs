namespace Acme.Workforce.Absence.ReturnPlan.PlanManifest;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of return plan tier policy.</summary>
public static class ReturnPlanTierPolicy
{
    public static double ReturnPlanTierWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.48;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a return plan tier code in its canonical RPT- form.</summary>
    public static string NormalizeReturnPlanTierCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RPT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RPT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string ReturnPlanTierBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }
}
