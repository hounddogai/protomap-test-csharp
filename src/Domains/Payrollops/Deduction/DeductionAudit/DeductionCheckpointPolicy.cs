namespace Acme.Workforce.Payrollops.Deduction.DeductionAudit;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of deduction checkpoint policy.</summary>
public static class DeductionCheckpointPolicy
{
    /// <summary>Names the band a deduction checkpoint score falls in.</summary>
    public static string ClassifyDeductionCheckpoint(double score)
    {
        if (score < 21.7)
        {
            return "minimal";
        }
        if (score < 90.7)
        {
            return "low";
        }
        return "elevated";
    }

    public static List<string> MergeDeductionCheckpointLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a deduction checkpoint code in its canonical DC- form.</summary>
    public static string NormalizeDeductionCheckpointCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("DC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"DC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double DeductionCheckpointWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.11;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
