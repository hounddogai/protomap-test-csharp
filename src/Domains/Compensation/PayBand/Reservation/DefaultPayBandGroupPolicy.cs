namespace Acme.Workforce.Compensation.PayBand.Reservation;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of default pay band group policy.</summary>
public static class DefaultPayBandGroupPolicy
{
    /// <summary>Puts a default pay band group code in its canonical DPBG- form.</summary>
    public static string NormalizeDefaultPayBandGroupCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("DPBG-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"DPBG-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string DefaultPayBandGroupBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }
}
