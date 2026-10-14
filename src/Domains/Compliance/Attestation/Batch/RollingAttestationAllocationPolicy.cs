namespace Acme.Workforce.Compliance.Attestation.Batch;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of rolling attestation allocation policy.</summary>
public static class RollingAttestationAllocationPolicy
{
    public static string RollingAttestationAllocationBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    public static List<string> MergeRollingAttestationAllocationLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double RollingAttestationAllocationWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.29;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a rolling attestation allocation code in its canonical RAA- form.</summary>
    public static string NormalizeRollingAttestationAllocationCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RAA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RAA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
