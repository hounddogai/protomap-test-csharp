namespace Acme.Workforce.Compliance.TrainingDue.Batch;

using Acme.Workforce.Compliance.PolicyAck.AckQuota;

/// <summary>Functions of training due preference from claim.</summary>
public static class TrainingDuePreferenceFromClaim
{
    public static Dictionary<string, double> SummarizeLegacyPolicyAckClaimsForPreference(List<LegacyPolicyAckClaim> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedExposure();
        }
        return totals;
    }

    public static List<TrainingDuePreference> DeriveTrainingDuePreferencesFromClaim(List<LegacyPolicyAckClaim> items)
    {
        var outValue = new List<TrainingDuePreference>();
        foreach (var item in items)
        {
            var derived = TrainingDuePreference.NewTrainingDuePreference(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AllocateTrainingDuePreference(TrainingDuePreference item)
    {
        return ScheduledPolicyAckQuotaPolicy.ScheduledPolicyAckQuotaBucket(item.MeasuredValue());
    }
}
