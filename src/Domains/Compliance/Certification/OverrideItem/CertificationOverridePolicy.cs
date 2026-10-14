namespace Acme.Workforce.Compliance.Certification.OverrideItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of certification override policy.</summary>
public static class CertificationOverridePolicy
{
    public static string CertificationOverrideBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Puts a certification override code in its canonical CO- form.</summary>
    public static string NormalizeCertificationOverrideCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CO-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CO-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
