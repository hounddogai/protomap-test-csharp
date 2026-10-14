namespace Acme.Workforce.Compliance.PolicyAck.Estimate;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of seasonal policy ack estimate policy.</summary>
public static class SeasonalPolicyAckEstimatePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 10829.0, in order.</summary>
    public static int SeasonalPolicyAckEstimateBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 10829.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double SeasonalPolicyAckEstimateWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.86;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
