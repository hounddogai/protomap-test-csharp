namespace Acme.Workforce.Compensation.BonusPool.Request;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of primary bonus pool notice policy.</summary>
public static class PrimaryBonusPoolNoticePolicy
{
    /// <summary>Puts a primary bonus pool notice code in its canonical PBPN- form.</summary>
    public static string NormalizePrimaryBonusPoolNoticeCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PBPN-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PBPN-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string PrimaryBonusPoolNoticeBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
