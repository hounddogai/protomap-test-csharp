namespace Acme.Workforce.Engagement.PulseSurvey.Assignment;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of pulse survey score policy.</summary>
public static class PulseSurveyScorePolicy
{
    /// <summary>Names the band a pulse survey score score falls in.</summary>
    public static string ClassifyPulseSurveyScore(double score)
    {
        if (score < 16.9)
        {
            return "medium";
        }
        if (score < 66.4)
        {
            return "critical";
        }
        return "low";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePulseSurveyScore()
    {
        var policy = Retry.DefaultPolicy("pulse-survey-score");
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

    /// <summary>Puts a pulse survey score code in its canonical PSS- form.</summary>
    public static string NormalizePulseSurveyScoreCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PSS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PSS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
