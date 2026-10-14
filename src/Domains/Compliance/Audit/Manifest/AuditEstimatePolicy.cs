namespace Acme.Workforce.Compliance.Audit.Manifest;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of audit estimate policy.</summary>
public static class AuditEstimatePolicy
{
    /// <summary>Puts a audit estimate code in its canonical AE- form.</summary>
    public static string NormalizeAuditEstimateCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string AuditEstimateBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
