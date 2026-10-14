namespace Acme.Workforce.Compensation.Adjustment.AdjustmentRule;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of adaptive adjustment notice policy.</summary>
public static class AdaptiveAdjustmentNoticePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 97836.0, in order.</summary>
    public static int AdaptiveAdjustmentNoticeBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 97836.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string AdaptiveAdjustmentNoticeBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }
}
