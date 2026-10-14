namespace Acme.Workforce.Orgdesign.Position.Template;

/// <summary>Functions of position cycle policy.</summary>
public static class PositionCyclePolicy
{
    /// <summary>Names the band a position cycle score falls in.</summary>
    public static string ClassifyPositionCycle(double score)
    {
        if (score < 10.3)
        {
            return "critical";
        }
        if (score < 34.3)
        {
            return "minimal";
        }
        if (score < 44.0)
        {
            return "medium";
        }
        if (score < 95.4)
        {
            return "ok";
        }
        return "elevated";
    }

    /// <summary>Counts how many amounts fit in a budget of 14757.0, in order.</summary>
    public static int PositionCycleBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 14757.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
