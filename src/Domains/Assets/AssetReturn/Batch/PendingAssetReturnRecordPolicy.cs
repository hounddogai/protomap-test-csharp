namespace Acme.Workforce.Assets.AssetReturn.Batch;

/// <summary>Functions of pending asset return record policy.</summary>
public static class PendingAssetReturnRecordPolicy
{
    /// <summary>Puts a pending asset return record code in its canonical PARR- form.</summary>
    public static string NormalizePendingAssetReturnRecordCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PARR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PARR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int PendingAssetReturnRecordSpan(int start, int end)
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
        return span / 86400;
    }
}
