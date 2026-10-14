namespace Acme.Workforce.Workplace.Desk.Tier;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of regional desk mapping policy.</summary>
public static class RegionalDeskMappingPolicy
{
    /// <summary>Puts a regional desk mapping code in its canonical RDM- form.</summary>
    public static string NormalizeRegionalDeskMappingCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RDM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RDM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRegionalDeskMapping()
    {
        var policy = Retry.DefaultPolicy("regional-desk-mapping");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 2);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Names the band a regional desk mapping score falls in.</summary>
    public static string ClassifyRegionalDeskMapping(double score)
    {
        if (score < 6.9)
        {
            return "elevated";
        }
        if (score < 19.7)
        {
            return "critical";
        }
        if (score < 84.6)
        {
            return "watch";
        }
        if (score < 91.6)
        {
            return "ok";
        }
        return "minimal";
    }
}
