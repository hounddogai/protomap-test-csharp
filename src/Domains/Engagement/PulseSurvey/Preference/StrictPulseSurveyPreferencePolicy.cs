namespace Acme.Workforce.Engagement.PulseSurvey.Preference;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of strict pulse survey preference policy.</summary>
public static class StrictPulseSurveyPreferencePolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeStrictPulseSurveyPreference()
    {
        var policy = Retry.DefaultPolicy("strict-pulse-survey-preference");
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

    /// <summary>Names the band a strict pulse survey preference score falls in.</summary>
    public static string ClassifyStrictPulseSurveyPreference(double score)
    {
        if (score < 15.1)
        {
            return "minimal";
        }
        if (score < 49.7)
        {
            return "ok";
        }
        if (score < 86.0)
        {
            return "watch";
        }
        if (score < 92.4)
        {
            return "elevated";
        }
        return "low";
    }
}
