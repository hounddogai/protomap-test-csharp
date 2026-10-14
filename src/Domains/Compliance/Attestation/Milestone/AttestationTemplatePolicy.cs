namespace Acme.Workforce.Compliance.Attestation.Milestone;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of attestation template policy.</summary>
public static class AttestationTemplatePolicy
{
    /// <summary>Puts a attestation template code in its canonical AT- form.</summary>
    public static string NormalizeAttestationTemplateCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double AttestationTemplateWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.69;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static List<string> MergeAttestationTemplateLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
