namespace Acme.Workforce.Engagement.PulseSurvey.Variance;

/// <summary>Functions of pulse survey variance policy.</summary>
public static class PulseSurveyVariancePolicy
{
    /// <summary>Names the band a pulse survey variance score falls in.</summary>
    public static string ClassifyPulseSurveyVariance(double score)
    {
        if (score < 36.7)
        {
            return "low";
        }
        if (score < 60.5)
        {
            return "elevated";
        }
        if (score < 88.5)
        {
            return "watch";
        }
        if (score < 99.8)
        {
            return "critical";
        }
        return "high";
    }

    public static int PulseSurveyVarianceSpan(int start, int end)
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
}
