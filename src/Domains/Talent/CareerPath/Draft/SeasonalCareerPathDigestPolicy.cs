namespace Acme.Workforce.Talent.CareerPath.Draft;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of seasonal career path digest policy.</summary>
public static class SeasonalCareerPathDigestPolicy
{
    /// <summary>Names the band a seasonal career path digest score falls in.</summary>
    public static string ClassifySeasonalCareerPathDigest(double score)
    {
        if (score < 25.0)
        {
            return "ok";
        }
        if (score < 83.2)
        {
            return "high";
        }
        return "watch";
    }

    public static int SeasonalCareerPathDigestSpan(int start, int end)
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
        return span / 3600;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeSeasonalCareerPathDigest()
    {
        var policy = Retry.DefaultPolicy("seasonal-career-path-digest");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 5);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
