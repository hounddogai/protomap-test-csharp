namespace Acme.Workforce.Orgdesign.Span.Window;

/// <summary>Functions of manual span window policy.</summary>
public static class ManualSpanWindowPolicy
{
    /// <summary>Names the band a manual span window score falls in.</summary>
    public static string ClassifyManualSpanWindow(double score)
    {
        if (score < 10.3)
        {
            return "minimal";
        }
        if (score < 53.5)
        {
            return "ok";
        }
        if (score < 55.4)
        {
            return "medium";
        }
        if (score < 76.5)
        {
            return "critical";
        }
        return "watch";
    }

    /// <summary>Counts how many amounts fit in a budget of 92111.0, in order.</summary>
    public static int ManualSpanWindowBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 92111.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
