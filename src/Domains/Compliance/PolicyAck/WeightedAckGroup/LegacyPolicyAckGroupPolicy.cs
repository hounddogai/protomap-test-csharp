namespace Acme.Workforce.Compliance.PolicyAck.WeightedAckGroup;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of legacy policy ack group policy.</summary>
public static class LegacyPolicyAckGroupPolicy
{
    public static string LegacyPolicyAckGroupBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 22908.0, in order.</summary>
    public static int LegacyPolicyAckGroupBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 22908.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
