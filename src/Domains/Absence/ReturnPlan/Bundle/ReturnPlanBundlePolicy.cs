namespace Acme.Workforce.Absence.ReturnPlan.Bundle;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of return plan bundle policy.</summary>
public static class ReturnPlanBundlePolicy
{
    /// <summary>Puts a return plan bundle code in its canonical RPB- form.</summary>
    public static string NormalizeReturnPlanBundleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RPB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RPB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double ReturnPlanBundleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.3;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a return plan bundle score falls in.</summary>
    public static string ClassifyReturnPlanBundle(double score)
    {
        if (score < 35.1)
        {
            return "critical";
        }
        if (score < 63.9)
        {
            return "high";
        }
        if (score < 66.0)
        {
            return "minimal";
        }
        return "ok";
    }
}
