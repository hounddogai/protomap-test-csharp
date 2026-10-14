namespace Acme.Workforce.Engagement.PulseSurvey.Audit;

/// <summary>Functions of pulse survey segment policy.</summary>
public static class PulseSurveySegmentPolicy
{
    /// <summary>Names the band a pulse survey segment score falls in.</summary>
    public static string ClassifyPulseSurveySegment(double score)
    {
        if (score < 2.1)
        {
            return "ok";
        }
        if (score < 12.6)
        {
            return "watch";
        }
        if (score < 24.0)
        {
            return "critical";
        }
        if (score < 59.4)
        {
            return "low";
        }
        return "high";
    }

    /// <summary>Puts a pulse survey segment code in its canonical PSS- form.</summary>
    public static string NormalizePulseSurveySegmentCode(string code)
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

    /// <summary>Counts how many amounts fit in a budget of 56183.0, in order.</summary>
    public static int PulseSurveySegmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 56183.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
