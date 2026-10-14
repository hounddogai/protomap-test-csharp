namespace Acme.Workforce.Talent.Nomination.Cycle;

/// <summary>Functions of nomination forecast policy.</summary>
public static class NominationForecastPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 41286.0, in order.</summary>
    public static int NominationForecastBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 41286.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a nomination forecast score falls in.</summary>
    public static string ClassifyNominationForecast(double score)
    {
        if (score < 31.9)
        {
            return "high";
        }
        if (score < 55.4)
        {
            return "elevated";
        }
        if (score < 76.0)
        {
            return "watch";
        }
        return "medium";
    }
}
