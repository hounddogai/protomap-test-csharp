namespace Acme.Workforce.Engagement.PulseSurvey.Exception;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of pulse survey tier policy.</summary>
public static class PulseSurveyTierPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 48812.0, in order.</summary>
    public static int PulseSurveyTierBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 48812.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string PulseSurveyTierBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}
