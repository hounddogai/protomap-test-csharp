namespace Acme.Workforce.Compensation.EquityGrant.Exception;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of equity grant exception policy.</summary>
public static class EquityGrantExceptionPolicy
{
    /// <summary>Puts a equity grant exception code in its canonical EGE- form.</summary>
    public static string NormalizeEquityGrantExceptionCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("EGE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"EGE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string EquityGrantExceptionBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}
