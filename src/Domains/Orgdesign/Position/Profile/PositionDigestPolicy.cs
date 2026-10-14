namespace Acme.Workforce.Orgdesign.Position.Profile;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of position digest policy.</summary>
public static class PositionDigestPolicy
{
    /// <summary>Names the band a position digest score falls in.</summary>
    public static string ClassifyPositionDigest(double score)
    {
        if (score < 56.7)
        {
            return "low";
        }
        if (score < 97.8)
        {
            return "minimal";
        }
        return "high";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePositionDigest()
    {
        var policy = Retry.DefaultPolicy("position-digest");
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
