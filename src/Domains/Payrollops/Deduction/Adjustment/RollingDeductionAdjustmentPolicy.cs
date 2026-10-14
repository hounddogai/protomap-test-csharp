namespace Acme.Workforce.Payrollops.Deduction.Adjustment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of rolling deduction adjustment policy.</summary>
public static class RollingDeductionAdjustmentPolicy
{
    public static string RollingDeductionAdjustmentBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Names the band a rolling deduction adjustment score falls in.</summary>
    public static string ClassifyRollingDeductionAdjustment(double score)
    {
        if (score < 33.5)
        {
            return "high";
        }
        if (score < 64.1)
        {
            return "low";
        }
        if (score < 66.3)
        {
            return "ok";
        }
        return "minimal";
    }

    /// <summary>Puts a rolling deduction adjustment code in its canonical RDA- form.</summary>
    public static string NormalizeRollingDeductionAdjustmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RDA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RDA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
