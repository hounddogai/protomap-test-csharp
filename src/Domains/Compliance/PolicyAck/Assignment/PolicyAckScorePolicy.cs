namespace Acme.Workforce.Compliance.PolicyAck.Assignment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of policy ack score policy.</summary>
public static class PolicyAckScorePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 92021.0, in order.</summary>
    public static int PolicyAckScoreBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 92021.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double PolicyAckScoreWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.35;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int PolicyAckScoreSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 86400;
    }
}
