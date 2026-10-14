namespace Acme.Workforce.Engagement.PulseSurvey.SurveyTier;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of seasonal pulse survey quota policy.</summary>
public static class SeasonalPulseSurveyQuotaPolicy
{
    public static string SeasonalPulseSurveyQuotaBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeSeasonalPulseSurveyQuota()
    {
        var policy = Retry.DefaultPolicy("seasonal-pulse-survey-quota");
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

    /// <summary>Counts how many amounts fit in a budget of 71250.0, in order.</summary>
    public static int SeasonalPulseSurveyQuotaBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 71250.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a seasonal pulse survey quota score falls in.</summary>
    public static string ClassifySeasonalPulseSurveyQuota(double score)
    {
        if (score < 33.9)
        {
            return "watch";
        }
        if (score < 37.4)
        {
            return "elevated";
        }
        return "medium";
    }
}
