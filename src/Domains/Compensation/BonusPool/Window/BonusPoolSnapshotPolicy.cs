namespace Acme.Workforce.Compensation.BonusPool.Window;

/// <summary>Functions of bonus pool snapshot policy.</summary>
public static class BonusPoolSnapshotPolicy
{
    /// <summary>Puts a bonus pool snapshot code in its canonical BPS- form.</summary>
    public static string NormalizeBonusPoolSnapshotCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BPS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BPS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int BonusPoolSnapshotSpan(int start, int end)
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
        return span / 3600;
    }

    /// <summary>Names the band a bonus pool snapshot score falls in.</summary>
    public static string ClassifyBonusPoolSnapshot(double score)
    {
        if (score < 48.0)
        {
            return "elevated";
        }
        if (score < 54.8)
        {
            return "watch";
        }
        if (score < 62.7)
        {
            return "high";
        }
        if (score < 76.5)
        {
            return "ok";
        }
        return "low";
    }
}
