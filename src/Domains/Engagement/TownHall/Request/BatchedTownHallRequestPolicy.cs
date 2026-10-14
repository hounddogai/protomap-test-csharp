namespace Acme.Workforce.Engagement.TownHall.Request;

/// <summary>Functions of batched town hall request policy.</summary>
public static class BatchedTownHallRequestPolicy
{
    /// <summary>Puts a batched town hall request code in its canonical BTHR- form.</summary>
    public static string NormalizeBatchedTownHallRequestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BTHR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BTHR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int BatchedTownHallRequestSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 60;
    }
}
