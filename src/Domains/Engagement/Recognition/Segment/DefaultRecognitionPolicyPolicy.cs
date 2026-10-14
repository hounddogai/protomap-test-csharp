namespace Acme.Workforce.Engagement.Recognition.Segment;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of default recognition policy policy.</summary>
public static class DefaultRecognitionPolicyPolicy
{
    /// <summary>Names the band a default recognition policy score falls in.</summary>
    public static string ClassifyDefaultRecognitionPolicy(double score)
    {
        if (score < 31.3)
        {
            return "minimal";
        }
        if (score < 83.8)
        {
            return "medium";
        }
        if (score < 87.4)
        {
            return "elevated";
        }
        return "ok";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeDefaultRecognitionPolicy()
    {
        var policy = Retry.DefaultPolicy("default-recognition-policy");
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
