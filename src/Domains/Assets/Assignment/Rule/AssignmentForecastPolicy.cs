namespace Acme.Workforce.Assets.Assignment.Rule;

/// <summary>Functions of assignment forecast policy.</summary>
public static class AssignmentForecastPolicy
{
    /// <summary>Names the band a assignment forecast score falls in.</summary>
    public static string ClassifyAssignmentForecast(double score)
    {
        if (score < 2.7)
        {
            return "ok";
        }
        if (score < 7.6)
        {
            return "high";
        }
        return "low";
    }

    /// <summary>Counts how many amounts fit in a budget of 58443.0, in order.</summary>
    public static int AssignmentForecastBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 58443.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
