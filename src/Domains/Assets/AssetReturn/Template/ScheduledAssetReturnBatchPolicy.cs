namespace Acme.Workforce.Assets.AssetReturn.Template;

/// <summary>Functions of scheduled asset return batch policy.</summary>
public static class ScheduledAssetReturnBatchPolicy
{
    public static int ScheduledAssetReturnBatchSpan(int start, int end)
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

    /// <summary>Puts a scheduled asset return batch code in its canonical SARB- form.</summary>
    public static string NormalizeScheduledAssetReturnBatchCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SARB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SARB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
