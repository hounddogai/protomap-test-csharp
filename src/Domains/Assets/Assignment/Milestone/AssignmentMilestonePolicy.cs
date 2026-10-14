namespace Acme.Workforce.Assets.Assignment.Milestone;

/// <summary>Functions of assignment milestone policy.</summary>
public static class AssignmentMilestonePolicy
{
    /// <summary>Names the band a assignment milestone score falls in.</summary>
    public static string ClassifyAssignmentMilestone(double score)
    {
        if (score < 14.9)
        {
            return "ok";
        }
        if (score < 89.4)
        {
            return "medium";
        }
        if (score < 98.6)
        {
            return "low";
        }
        return "elevated";
    }

    /// <summary>Counts how many amounts fit in a budget of 46953.0, in order.</summary>
    public static int AssignmentMilestoneBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 46953.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
