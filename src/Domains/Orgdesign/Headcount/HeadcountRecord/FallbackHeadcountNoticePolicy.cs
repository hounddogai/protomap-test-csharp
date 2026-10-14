namespace Acme.Workforce.Orgdesign.Headcount.HeadcountRecord;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of fallback headcount notice policy.</summary>
public static class FallbackHeadcountNoticePolicy
{
    /// <summary>Puts a fallback headcount notice code in its canonical FHN- form.</summary>
    public static string NormalizeFallbackHeadcountNoticeCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FHN-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FHN-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a fallback headcount notice score falls in.</summary>
    public static string ClassifyFallbackHeadcountNotice(double score)
    {
        if (score < 27.8)
        {
            return "watch";
        }
        if (score < 38.9)
        {
            return "medium";
        }
        if (score < 72.5)
        {
            return "ok";
        }
        return "low";
    }

    public static double FallbackHeadcountNoticeWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.24;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeFallbackHeadcountNotice()
    {
        var policy = Retry.DefaultPolicy("fallback-headcount-notice");
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
}
