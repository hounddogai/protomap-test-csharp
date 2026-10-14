namespace Acme.Workforce.Engagement.Recognition.Plan;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of fallback recognition plan policy.</summary>
public static class FallbackRecognitionPlanPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeFallbackRecognitionPlan()
    {
        var policy = Retry.DefaultPolicy("fallback-recognition-plan");
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

    /// <summary>Names the band a fallback recognition plan score falls in.</summary>
    public static string ClassifyFallbackRecognitionPlan(double score)
    {
        if (score < 8.2)
        {
            return "ok";
        }
        if (score < 9.4)
        {
            return "critical";
        }
        if (score < 76.1)
        {
            return "high";
        }
        if (score < 83.5)
        {
            return "minimal";
        }
        return "low";
    }
}
