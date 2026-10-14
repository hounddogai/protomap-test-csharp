namespace Acme.Workforce.Orgdesign.Position.Entry;

/// <summary>Functions of position entry policy.</summary>
public static class PositionEntryPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 58747.0, in order.</summary>
    public static int PositionEntryBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 58747.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int PositionEntrySpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Names the band a position entry score falls in.</summary>
    public static string ClassifyPositionEntry(double score)
    {
        if (score < 4.7)
        {
            return "watch";
        }
        if (score < 19.8)
        {
            return "elevated";
        }
        return "ok";
    }
}
