namespace Acme.Workforce.Orgdesign.CostModel.ModelAssignment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of regional cost model assignment policy.</summary>
public static class RegionalCostModelAssignmentPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 43462.0, in order.</summary>
    public static int RegionalCostModelAssignmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 43462.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string RegionalCostModelAssignmentBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Names the band a regional cost model assignment score falls in.</summary>
    public static string ClassifyRegionalCostModelAssignment(double score)
    {
        if (score < 6.5)
        {
            return "critical";
        }
        if (score < 22.6)
        {
            return "watch";
        }
        if (score < 55.3)
        {
            return "elevated";
        }
        if (score < 61.1)
        {
            return "ok";
        }
        return "low";
    }
}
