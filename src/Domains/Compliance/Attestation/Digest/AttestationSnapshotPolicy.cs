namespace Acme.Workforce.Compliance.Attestation.Digest;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of attestation snapshot policy.</summary>
public static class AttestationSnapshotPolicy
{
    /// <summary>Puts a attestation snapshot code in its canonical AS- form.</summary>
    public static string NormalizeAttestationSnapshotCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string AttestationSnapshotBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
