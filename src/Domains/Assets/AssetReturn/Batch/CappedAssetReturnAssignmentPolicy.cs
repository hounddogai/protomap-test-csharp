namespace Acme.Workforce.Assets.AssetReturn.Batch;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of capped asset return assignment policy.</summary>
public static class CappedAssetReturnAssignmentPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 68237.0, in order.</summary>
    public static int CappedAssetReturnAssignmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 68237.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string CappedAssetReturnAssignmentBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Names the band a capped asset return assignment score falls in.</summary>
    public static string ClassifyCappedAssetReturnAssignment(double score)
    {
        if (score < 5.7)
        {
            return "critical";
        }
        if (score < 74.3)
        {
            return "elevated";
        }
        return "ok";
    }
}
