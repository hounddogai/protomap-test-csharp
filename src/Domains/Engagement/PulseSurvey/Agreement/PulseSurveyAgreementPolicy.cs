namespace Acme.Workforce.Engagement.PulseSurvey.Agreement;

/// <summary>Functions of pulse survey agreement policy.</summary>
public static class PulseSurveyAgreementPolicy
{
    /// <summary>Names the band a pulse survey agreement score falls in.</summary>
    public static string ClassifyPulseSurveyAgreement(double score)
    {
        if (score < 35.6)
        {
            return "critical";
        }
        if (score < 93.3)
        {
            return "minimal";
        }
        return "high";
    }

    public static int PulseSurveyAgreementSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 86400;
    }
}
