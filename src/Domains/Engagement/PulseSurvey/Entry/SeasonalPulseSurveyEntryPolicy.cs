namespace Acme.Workforce.Engagement.PulseSurvey.Entry;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of seasonal pulse survey entry policy.</summary>
public static class SeasonalPulseSurveyEntryPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 84092.0, in order.</summary>
    public static int SeasonalPulseSurveyEntryBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 84092.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double SeasonalPulseSurveyEntryWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.14;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
