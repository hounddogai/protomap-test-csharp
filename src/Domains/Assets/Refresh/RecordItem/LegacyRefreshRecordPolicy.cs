namespace Acme.Workforce.Assets.Refresh.RecordItem;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of legacy refresh record policy.</summary>
public static class LegacyRefreshRecordPolicy
{
    /// <summary>Names the band a legacy refresh record score falls in.</summary>
    public static string ClassifyLegacyRefreshRecord(double score)
    {
        if (score < 27.6)
        {
            return "low";
        }
        if (score < 38.8)
        {
            return "critical";
        }
        if (score < 83.0)
        {
            return "high";
        }
        return "ok";
    }

    /// <summary>Puts a legacy refresh record code in its canonical LRR- form.</summary>
    public static string NormalizeLegacyRefreshRecordCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LRR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LRR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLegacyRefreshRecord()
    {
        var policy = Retry.DefaultPolicy("legacy-refresh-record");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 4);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static int LegacyRefreshRecordSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 86400;
    }
}
