namespace Acme.Workforce.Engagement.PulseSurvey.Assignment;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of strict pulse survey draft policy.</summary>
public static class StrictPulseSurveyDraftPolicy
{
    public static int StrictPulseSurveyDraftSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeStrictPulseSurveyDraft()
    {
        var policy = Retry.DefaultPolicy("strict-pulse-survey-draft");
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
