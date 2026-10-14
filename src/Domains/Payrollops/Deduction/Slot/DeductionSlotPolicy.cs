namespace Acme.Workforce.Payrollops.Deduction.Slot;

/// <summary>Functions of deduction slot policy.</summary>
public static class DeductionSlotPolicy
{
    /// <summary>Names the band a deduction slot score falls in.</summary>
    public static string ClassifyDeductionSlot(double score)
    {
        if (score < 5.1)
        {
            return "ok";
        }
        if (score < 17.6)
        {
            return "minimal";
        }
        if (score < 47.3)
        {
            return "critical";
        }
        if (score < 93.5)
        {
            return "low";
        }
        return "elevated";
    }

    /// <summary>Puts a deduction slot code in its canonical DS- form.</summary>
    public static string NormalizeDeductionSlotCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("DS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"DS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
