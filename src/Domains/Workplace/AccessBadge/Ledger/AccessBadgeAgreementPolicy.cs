namespace Acme.Workforce.Workplace.AccessBadge.Ledger;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of access badge agreement policy.</summary>
public static class AccessBadgeAgreementPolicy
{
    public static string AccessBadgeAgreementBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Puts a access badge agreement code in its canonical ABA- form.</summary>
    public static string NormalizeAccessBadgeAgreementCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("ABA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"ABA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
