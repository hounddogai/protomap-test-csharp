namespace Acme.Workforce.Engagement.PulseSurvey.Quota;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of manual pulse survey quota policy.</summary>
public static class ManualPulseSurveyQuotaPolicy
{
    /// <summary>Names the band a manual pulse survey quota score falls in.</summary>
    public static string ClassifyManualPulseSurveyQuota(double score)
    {
        if (score < 34.8)
        {
            return "elevated";
        }
        if (score < 63.3)
        {
            return "medium";
        }
        if (score < 63.9)
        {
            return "watch";
        }
        if (score < 70.9)
        {
            return "low";
        }
        return "critical";
    }

    public static string ManualPulseSurveyQuotaBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 84286.0, in order.</summary>
    public static int ManualPulseSurveyQuotaBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 84286.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
