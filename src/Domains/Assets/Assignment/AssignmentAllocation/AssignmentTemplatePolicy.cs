namespace Acme.Workforce.Assets.Assignment.AssignmentAllocation;

/// <summary>Functions of assignment template policy.</summary>
public static class AssignmentTemplatePolicy
{
    /// <summary>Names the band a assignment template score falls in.</summary>
    public static string ClassifyAssignmentTemplate(double score)
    {
        if (score < 4.5)
        {
            return "elevated";
        }
        if (score < 14.1)
        {
            return "ok";
        }
        if (score < 20.0)
        {
            return "medium";
        }
        if (score < 71.9)
        {
            return "watch";
        }
        return "minimal";
    }

    /// <summary>Counts how many amounts fit in a budget of 4941.0, in order.</summary>
    public static int AssignmentTemplateBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 4941.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
