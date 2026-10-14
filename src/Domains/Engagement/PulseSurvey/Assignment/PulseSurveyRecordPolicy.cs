namespace Acme.Workforce.Engagement.PulseSurvey.Assignment;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of pulse survey record policy.</summary>
public static class PulseSurveyRecordPolicy
{
    public static string PulseSurveyRecordBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePulseSurveyRecord()
    {
        var policy = Retry.DefaultPolicy("pulse-survey-record");
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

    /// <summary>Names the band a pulse survey record score falls in.</summary>
    public static string ClassifyPulseSurveyRecord(double score)
    {
        if (score < 51.9)
        {
            return "elevated";
        }
        if (score < 69.5)
        {
            return "medium";
        }
        if (score < 75.5)
        {
            return "watch";
        }
        if (score < 94.8)
        {
            return "high";
        }
        return "ok";
    }

    /// <summary>Puts a pulse survey record code in its canonical PSR- form.</summary>
    public static string NormalizePulseSurveyRecordCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PSR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PSR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
