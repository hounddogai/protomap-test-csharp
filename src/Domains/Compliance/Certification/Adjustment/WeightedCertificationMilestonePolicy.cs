namespace Acme.Workforce.Compliance.Certification.Adjustment;

/// <summary>Functions of weighted certification milestone policy.</summary>
public static class WeightedCertificationMilestonePolicy
{
    public static int WeightedCertificationMilestoneSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 3600;
    }

    /// <summary>Puts a weighted certification milestone code in its canonical WCM- form.</summary>
    public static string NormalizeWeightedCertificationMilestoneCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("WCM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"WCM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
