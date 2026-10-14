namespace Acme.Workforce.Assets.LicenseSeat.SeatAgreement;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of adaptive license seat agreement policy.</summary>
public static class AdaptiveLicenseSeatAgreementPolicy
{
    /// <summary>Puts a adaptive license seat agreement code in its canonical ALSA- form.</summary>
    public static string NormalizeAdaptiveLicenseSeatAgreementCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("ALSA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"ALSA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int AdaptiveLicenseSeatAgreementSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }

    public static string AdaptiveLicenseSeatAgreementBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Names the band a adaptive license seat agreement score falls in.</summary>
    public static string ClassifyAdaptiveLicenseSeatAgreement(double score)
    {
        if (score < 82.4)
        {
            return "minimal";
        }
        if (score < 98.0)
        {
            return "medium";
        }
        return "watch";
    }
}
