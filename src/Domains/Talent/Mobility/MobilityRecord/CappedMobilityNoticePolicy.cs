namespace Acme.Workforce.Talent.Mobility.MobilityRecord;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of capped mobility notice policy.</summary>
public static class CappedMobilityNoticePolicy
{
    public static string CappedMobilityNoticeBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Puts a capped mobility notice code in its canonical CMN- form.</summary>
    public static string NormalizeCappedMobilityNoticeCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CMN-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CMN-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int CappedMobilityNoticeSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 3600;
    }
}
