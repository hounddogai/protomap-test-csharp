namespace Acme.Workforce.Absence.Accrual.Assignment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of accrual group policy.</summary>
public static class AccrualGroupPolicy
{
    public static string AccrualGroupBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Names the band a accrual group score falls in.</summary>
    public static string ClassifyAccrualGroup(double score)
    {
        if (score < 21.5)
        {
            return "medium";
        }
        if (score < 23.2)
        {
            return "critical";
        }
        if (score < 35.7)
        {
            return "ok";
        }
        if (score < 66.3)
        {
            return "high";
        }
        return "watch";
    }
}
