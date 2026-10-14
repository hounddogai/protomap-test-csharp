namespace Acme.Workforce.Compliance.Certification.Request;

/// <summary>Functions of lenient certification assignment policy.</summary>
public static class LenientCertificationAssignmentPolicy
{
    /// <summary>Names the band a lenient certification assignment score falls in.</summary>
    public static string ClassifyLenientCertificationAssignment(double score)
    {
        if (score < 24.7)
        {
            return "low";
        }
        if (score < 42.1)
        {
            return "elevated";
        }
        if (score < 54.0)
        {
            return "medium";
        }
        if (score < 68.3)
        {
            return "critical";
        }
        return "watch";
    }

    /// <summary>Puts a lenient certification assignment code in its canonical LCA- form.</summary>
    public static string NormalizeLenientCertificationAssignmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LCA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LCA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
