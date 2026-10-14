namespace Acme.Workforce.Workplace.Visitor.Entry;

/// <summary>Functions of default visitor template policy.</summary>
public static class DefaultVisitorTemplatePolicy
{
    /// <summary>Names the band a default visitor template score falls in.</summary>
    public static string ClassifyDefaultVisitorTemplate(double score)
    {
        if (score < 13.8)
        {
            return "critical";
        }
        if (score < 44.5)
        {
            return "high";
        }
        return "elevated";
    }

    /// <summary>Counts how many amounts fit in a budget of 32091.0, in order.</summary>
    public static int DefaultVisitorTemplateBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 32091.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
