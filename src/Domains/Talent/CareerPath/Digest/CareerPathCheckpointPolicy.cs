namespace Acme.Workforce.Talent.CareerPath.Digest;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of career path checkpoint policy.</summary>
public static class CareerPathCheckpointPolicy
{
    /// <summary>Names the band a career path checkpoint score falls in.</summary>
    public static string ClassifyCareerPathCheckpoint(double score)
    {
        if (score < 43.6)
        {
            return "watch";
        }
        if (score < 52.5)
        {
            return "medium";
        }
        if (score < 96.2)
        {
            return "elevated";
        }
        return "high";
    }

    public static string CareerPathCheckpointBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeCareerPathCheckpoint()
    {
        var policy = Retry.DefaultPolicy("career-path-checkpoint");
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
}
