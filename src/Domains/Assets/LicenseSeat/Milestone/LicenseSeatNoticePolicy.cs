namespace Acme.Workforce.Assets.LicenseSeat.Milestone;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of license seat notice policy.</summary>
public static class LicenseSeatNoticePolicy
{
    /// <summary>Puts a license seat notice code in its canonical LSN- form.</summary>
    public static string NormalizeLicenseSeatNoticeCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LSN-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LSN-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double LicenseSeatNoticeWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.24;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a license seat notice score falls in.</summary>
    public static string ClassifyLicenseSeatNotice(double score)
    {
        if (score < 20.5)
        {
            return "medium";
        }
        if (score < 96.3)
        {
            return "watch";
        }
        return "high";
    }

    public static int LicenseSeatNoticeSpan(int start, int end)
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
        return span / 60;
    }
}
