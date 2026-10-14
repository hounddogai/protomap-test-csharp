namespace Acme.Workforce.Assets.AssetReturn.Summary;

/// <summary>Functions of asset return schedule policy.</summary>
public static class AssetReturnSchedulePolicy
{
    public static int AssetReturnScheduleSpan(int start, int end)
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
        return span / 3600;
    }

    /// <summary>Puts a asset return schedule code in its canonical ARS- form.</summary>
    public static string NormalizeAssetReturnScheduleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("ARS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"ARS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
