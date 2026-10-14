namespace Acme.Workforce.Orgdesign.CostModel.Rule;

/// <summary>Functions of legacy cost model manifest policy.</summary>
public static class LegacyCostModelManifestPolicy
{
    public static int LegacyCostModelManifestSpan(int start, int end)
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

    /// <summary>Names the band a legacy cost model manifest score falls in.</summary>
    public static string ClassifyLegacyCostModelManifest(double score)
    {
        if (score < 17.1)
        {
            return "watch";
        }
        if (score < 71.1)
        {
            return "ok";
        }
        return "minimal";
    }
}
