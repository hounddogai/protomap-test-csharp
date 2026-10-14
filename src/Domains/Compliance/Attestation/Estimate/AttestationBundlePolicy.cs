namespace Acme.Workforce.Compliance.Attestation.Estimate;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of attestation bundle policy.</summary>
public static class AttestationBundlePolicy
{
    public static List<string> MergeAttestationBundleLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a attestation bundle score falls in.</summary>
    public static string ClassifyAttestationBundle(double score)
    {
        if (score < 14.7)
        {
            return "critical";
        }
        if (score < 22.1)
        {
            return "elevated";
        }
        if (score < 56.0)
        {
            return "watch";
        }
        if (score < 60.2)
        {
            return "high";
        }
        return "medium";
    }

    /// <summary>Puts a attestation bundle code in its canonical AB- form.</summary>
    public static string NormalizeAttestationBundleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
