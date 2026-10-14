namespace Acme.Workforce.Compliance.PolicyAck.AckReservation;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of adaptive policy ack snapshot policy.</summary>
public static class AdaptivePolicyAckSnapshotPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 1446.0, in order.</summary>
    public static int AdaptivePolicyAckSnapshotBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 1446.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string AdaptivePolicyAckSnapshotBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }
}
