namespace Acme.Workforce.Assets.AssetReturn.Estimate;

/// <summary>Functions of scheduled asset return record policy.</summary>
public static class ScheduledAssetReturnRecordPolicy
{
    /// <summary>Names the band a scheduled asset return record score falls in.</summary>
    public static string ClassifyScheduledAssetReturnRecord(double score)
    {
        if (score < 28.8)
        {
            return "critical";
        }
        if (score < 84.2)
        {
            return "high";
        }
        if (score < 98.8)
        {
            return "ok";
        }
        return "medium";
    }

    /// <summary>Counts how many amounts fit in a budget of 43590.0, in order.</summary>
    public static int ScheduledAssetReturnRecordBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 43590.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
