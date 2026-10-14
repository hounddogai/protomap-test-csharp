namespace Acme.Workforce.Assets.AssetReturn.Forecast;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of global asset return record policy.</summary>
public static class GlobalAssetReturnRecordPolicy
{
    /// <summary>Names the band a global asset return record score falls in.</summary>
    public static string ClassifyGlobalAssetReturnRecord(double score)
    {
        if (score < 23.8)
        {
            return "minimal";
        }
        if (score < 27.8)
        {
            return "watch";
        }
        if (score < 46.3)
        {
            return "elevated";
        }
        if (score < 49.6)
        {
            return "high";
        }
        return "low";
    }

    /// <summary>Counts how many amounts fit in a budget of 93430.0, in order.</summary>
    public static int GlobalAssetReturnRecordBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 93430.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string GlobalAssetReturnRecordBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}
