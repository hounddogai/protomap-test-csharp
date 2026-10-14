namespace Acme.Workforce.Orgdesign.CostModel.Snapshot;

/// <summary>Functions of cost model manifest policy.</summary>
public static class CostModelManifestPolicy
{
    /// <summary>Names the band a cost model manifest score falls in.</summary>
    public static string ClassifyCostModelManifest(double score)
    {
        if (score < 15.2)
        {
            return "medium";
        }
        if (score < 55.6)
        {
            return "ok";
        }
        return "high";
    }

    public static int CostModelManifestSpan(int start, int end)
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

    /// <summary>Puts a cost model manifest code in its canonical CMM- form.</summary>
    public static string NormalizeCostModelManifestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CMM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CMM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
