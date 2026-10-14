namespace Acme.Workforce.Talent.Nomination.Request;

/// <summary>Functions of lenient nomination forecast policy.</summary>
public static class LenientNominationForecastPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 6329.0, in order.</summary>
    public static int LenientNominationForecastBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 6329.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a lenient nomination forecast score falls in.</summary>
    public static string ClassifyLenientNominationForecast(double score)
    {
        if (score < 15.1)
        {
            return "elevated";
        }
        if (score < 73.6)
        {
            return "critical";
        }
        if (score < 84.5)
        {
            return "minimal";
        }
        return "ok";
    }
}
