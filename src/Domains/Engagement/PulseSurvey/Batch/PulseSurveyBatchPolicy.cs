namespace Acme.Workforce.Engagement.PulseSurvey.Batch;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of pulse survey batch policy.</summary>
public static class PulseSurveyBatchPolicy
{
    public static double PulseSurveyBatchWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.89;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int PulseSurveyBatchSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Names the band a pulse survey batch score falls in.</summary>
    public static string ClassifyPulseSurveyBatch(double score)
    {
        if (score < 6.8)
        {
            return "high";
        }
        if (score < 14.1)
        {
            return "critical";
        }
        if (score < 93.8)
        {
            return "elevated";
        }
        return "ok";
    }

    /// <summary>Puts a pulse survey batch code in its canonical PSB- form.</summary>
    public static string NormalizePulseSurveyBatchCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PSB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PSB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
