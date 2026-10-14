namespace Acme.Workforce.Orgdesign.Position.Summary;

/// <summary>Functions of position summary policy.</summary>
public static class PositionSummaryPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 1825.0, in order.</summary>
    public static int PositionSummaryBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 1825.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a position summary score falls in.</summary>
    public static string ClassifyPositionSummary(double score)
    {
        if (score < 67.2)
        {
            return "low";
        }
        if (score < 74.1)
        {
            return "critical";
        }
        return "ok";
    }
}
