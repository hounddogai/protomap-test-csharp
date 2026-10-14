namespace Acme.Workforce.Absence.Accrual.Ledger;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of global accrual estimate policy.</summary>
public static class GlobalAccrualEstimatePolicy
{
    public static double GlobalAccrualEstimateWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.08;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a global accrual estimate score falls in.</summary>
    public static string ClassifyGlobalAccrualEstimate(double score)
    {
        if (score < 20.5)
        {
            return "medium";
        }
        if (score < 25.9)
        {
            return "minimal";
        }
        if (score < 81.8)
        {
            return "elevated";
        }
        return "critical";
    }

    public static string GlobalAccrualEstimateBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}
