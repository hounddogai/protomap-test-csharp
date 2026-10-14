namespace Acme.Workforce.Talent.Mobility.Tier;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of mobility record policy.</summary>
public static class MobilityRecordPolicy
{
    /// <summary>Puts a mobility record code in its canonical MR- form.</summary>
    public static string NormalizeMobilityRecordCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("MR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"MR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a mobility record score falls in.</summary>
    public static string ClassifyMobilityRecord(double score)
    {
        if (score < 37.6)
        {
            return "watch";
        }
        if (score < 45.0)
        {
            return "critical";
        }
        if (score < 55.2)
        {
            return "minimal";
        }
        return "ok";
    }

    public static int MobilityRecordSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeMobilityRecord()
    {
        var policy = Retry.DefaultPolicy("mobility-record");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 3);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
