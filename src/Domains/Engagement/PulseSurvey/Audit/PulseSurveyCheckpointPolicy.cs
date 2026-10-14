namespace Acme.Workforce.Engagement.PulseSurvey.Audit;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of pulse survey checkpoint policy.</summary>
public static class PulseSurveyCheckpointPolicy
{
    public static int PulseSurveyCheckpointSpan(int start, int end)
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

    /// <summary>Names the band a pulse survey checkpoint score falls in.</summary>
    public static string ClassifyPulseSurveyCheckpoint(double score)
    {
        if (score < 59.5)
        {
            return "ok";
        }
        if (score < 74.0)
        {
            return "medium";
        }
        if (score < 86.2)
        {
            return "elevated";
        }
        return "watch";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePulseSurveyCheckpoint()
    {
        var policy = Retry.DefaultPolicy("pulse-survey-checkpoint");
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
