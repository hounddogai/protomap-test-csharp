namespace Acme.Workforce.Workplace.Desk.Adjustment;

/// <summary>Functions of desk adjustment policy.</summary>
public static class DeskAdjustmentPolicy
{
    /// <summary>Names the band a desk adjustment score falls in.</summary>
    public static string ClassifyDeskAdjustment(double score)
    {
        if (score < 50.3)
        {
            return "ok";
        }
        if (score < 65.3)
        {
            return "watch";
        }
        return "medium";
    }

    /// <summary>Counts how many amounts fit in a budget of 48289.0, in order.</summary>
    public static int DeskAdjustmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 48289.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
