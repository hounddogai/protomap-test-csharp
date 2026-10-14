namespace Acme.Workforce.Assets.LicenseSeat.Audit;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of license seat profile policy.</summary>
public static class LicenseSeatProfilePolicy
{
    /// <summary>Puts a license seat profile code in its canonical LSP- form.</summary>
    public static string NormalizeLicenseSeatProfileCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LSP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LSP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string LicenseSeatProfileBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    public static int LicenseSeatProfileSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 86400;
    }
}
