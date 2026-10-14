namespace Acme.Workforce.Engagement.PulseSurvey.Tier;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of pulse survey slot policy.</summary>
public static class PulseSurveySlotPolicy
{
    public static string PulseSurveySlotBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Names the band a pulse survey slot score falls in.</summary>
    public static string ClassifyPulseSurveySlot(double score)
    {
        if (score < 45.5)
        {
            return "watch";
        }
        if (score < 72.0)
        {
            return "medium";
        }
        if (score < 76.2)
        {
            return "critical";
        }
        if (score < 98.7)
        {
            return "elevated";
        }
        return "high";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePulseSurveySlot()
    {
        var policy = Retry.DefaultPolicy("pulse-survey-slot");
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
