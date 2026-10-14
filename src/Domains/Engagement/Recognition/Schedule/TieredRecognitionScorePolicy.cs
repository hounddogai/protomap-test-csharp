namespace Acme.Workforce.Engagement.Recognition.Schedule;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of tiered recognition score policy.</summary>
public static class TieredRecognitionScorePolicy
{
    public static int TieredRecognitionScoreSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 3600;
    }

    public static string TieredRecognitionScoreBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Puts a tiered recognition score code in its canonical TRS- form.</summary>
    public static string NormalizeTieredRecognitionScoreCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TRS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TRS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
