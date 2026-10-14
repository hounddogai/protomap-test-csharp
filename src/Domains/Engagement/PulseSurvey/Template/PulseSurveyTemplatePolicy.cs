namespace Acme.Workforce.Engagement.PulseSurvey.Template;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of pulse survey template policy.</summary>
public static class PulseSurveyTemplatePolicy
{
    /// <summary>Names the band a pulse survey template score falls in.</summary>
    public static string ClassifyPulseSurveyTemplate(double score)
    {
        if (score < 53.7)
        {
            return "low";
        }
        if (score < 54.1)
        {
            return "watch";
        }
        if (score < 67.8)
        {
            return "elevated";
        }
        if (score < 89.1)
        {
            return "critical";
        }
        return "medium";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePulseSurveyTemplate()
    {
        var policy = Retry.DefaultPolicy("pulse-survey-template");
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

    /// <summary>Puts a pulse survey template code in its canonical PST- form.</summary>
    public static string NormalizePulseSurveyTemplateCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PST-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PST-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double PulseSurveyTemplateWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.66;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
