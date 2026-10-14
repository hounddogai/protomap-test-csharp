namespace Acme.Workforce.Compliance.Attestation.Quota;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of attestation budget policy.</summary>
public static class AttestationBudgetPolicy
{
    /// <summary>Names the band a attestation budget score falls in.</summary>
    public static string ClassifyAttestationBudget(double score)
    {
        if (score < 1.9)
        {
            return "high";
        }
        if (score < 3.6)
        {
            return "watch";
        }
        return "low";
    }

    /// <summary>Puts a attestation budget code in its canonical AB- form.</summary>
    public static string NormalizeAttestationBudgetCode(string code)
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

    public static string AttestationBudgetBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }
}
