namespace Acme.Workforce.Compensation.EquityGrant.Group;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of equity grant quota policy.</summary>
public static class EquityGrantQuotaPolicy
{
    /// <summary>Puts a equity grant quota code in its canonical EGQ- form.</summary>
    public static string NormalizeEquityGrantQuotaCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("EGQ-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"EGQ-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string EquityGrantQuotaBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
