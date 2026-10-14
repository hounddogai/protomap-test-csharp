namespace Acme.Workforce.Engagement.PulseSurvey.Metric;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of scheduled pulse survey estimate policy.</summary>
public static class ScheduledPulseSurveyEstimatePolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeScheduledPulseSurveyEstimate()
    {
        var policy = Retry.DefaultPolicy("scheduled-pulse-survey-estimate");
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

    public static double ScheduledPulseSurveyEstimateWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.82;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string ScheduledPulseSurveyEstimateBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }
}
