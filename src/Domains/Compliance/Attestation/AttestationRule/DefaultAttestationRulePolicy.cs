namespace Acme.Workforce.Compliance.Attestation.AttestationRule;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of default attestation rule policy.</summary>
public static class DefaultAttestationRulePolicy
{
    /// <summary>Puts a default attestation rule code in its canonical DAR- form.</summary>
    public static string NormalizeDefaultAttestationRuleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("DAR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"DAR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeDefaultAttestationRuleLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static string DefaultAttestationRuleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Names the band a default attestation rule score falls in.</summary>
    public static string ClassifyDefaultAttestationRule(double score)
    {
        if (score < 14.7)
        {
            return "medium";
        }
        if (score < 52.0)
        {
            return "minimal";
        }
        return "high";
    }
}
