namespace Acme.Workforce.Talent.CareerPath.Digest;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of primary career path digest policy.</summary>
public static class PrimaryCareerPathDigestPolicy
{
    /// <summary>Names the band a primary career path digest score falls in.</summary>
    public static string ClassifyPrimaryCareerPathDigest(double score)
    {
        if (score < 75.9)
        {
            return "elevated";
        }
        if (score < 77.6)
        {
            return "critical";
        }
        return "minimal";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePrimaryCareerPathDigest()
    {
        var policy = Retry.DefaultPolicy("primary-career-path-digest");
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

    public static int PrimaryCareerPathDigestSpan(int start, int end)
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
        return span / 60;
    }

    public static string PrimaryCareerPathDigestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
