namespace Acme.Workforce.Compliance.PolicyAck.AckProfile;

using Acme.Workforce.Compliance.TrainingDue.Window;

/// <summary>Functions of policy ack preference from cycle.</summary>
public static class PolicyAckPreferenceFromCycle
{
    public static Dictionary<string, double> SummarizeTrainingDueCyclesForPreference(List<TrainingDueCycle> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedLoad();
        }
        return totals;
    }

    public static List<PolicyAckPreference> DerivePolicyAckPreferencesFromCycle(List<TrainingDueCycle> items)
    {
        var outValue = new List<PolicyAckPreference>();
        foreach (var item in items)
        {
            var derived = PolicyAckPreference.NewPolicyAckPreference(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SettlePolicyAckPreference(PolicyAckPreference item)
    {
        return TrainingDueWindowPolicy.ClassifyTrainingDueWindow(item.EffectiveVolume());
    }
}
