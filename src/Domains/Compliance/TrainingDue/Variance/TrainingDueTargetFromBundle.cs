namespace Acme.Workforce.Compliance.TrainingDue.Variance;

using Acme.Workforce.Compliance.PolicyAck.Bundle;

/// <summary>Functions of training due target from bundle.</summary>
public static class TrainingDueTargetFromBundle
{
    public static Dictionary<string, double> SummarizePolicyAckBundlesForTarget(List<PolicyAckBundle> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveLoad();
        }
        return totals;
    }

    public static List<TrainingDueTarget> DeriveTrainingDueTargetsFromBundle(List<PolicyAckBundle> items)
    {
        var outValue = new List<TrainingDueTarget>();
        foreach (var item in items)
        {
            var derived = TrainingDueTarget.NewTrainingDueTarget(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SettleTrainingDueTarget(TrainingDueTarget item)
    {
        return PolicyAckBundlePolicy.PolicyAckBundleBucket(item.MeasuredValue());
    }
}
