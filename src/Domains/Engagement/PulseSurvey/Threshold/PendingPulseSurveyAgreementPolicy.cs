namespace Acme.Workforce.Engagement.PulseSurvey.Threshold;

/// <summary>Functions of pending pulse survey agreement policy.</summary>
public static class PendingPulseSurveyAgreementPolicy
{
    public static int PendingPulseSurveyAgreementSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Puts a pending pulse survey agreement code in its canonical PPSA- form.</summary>
    public static string NormalizePendingPulseSurveyAgreementCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PPSA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PPSA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a pending pulse survey agreement score falls in.</summary>
    public static string ClassifyPendingPulseSurveyAgreement(double score)
    {
        if (score < 3.4)
        {
            return "ok";
        }
        if (score < 22.6)
        {
            return "critical";
        }
        if (score < 33.8)
        {
            return "high";
        }
        return "watch";
    }

    /// <summary>Counts how many amounts fit in a budget of 37173.0, in order.</summary>
    public static int PendingPulseSurveyAgreementBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 37173.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
