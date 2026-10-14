namespace Acme.Workforce.Compliance.Attestation.Preference;

using Acme.Workforce.Compliance.TrainingDue.Budget;

/// <summary>Functions of regional attestation preference from slot.</summary>
public static class RegionalAttestationPreferenceFromSlot
{
    public static Dictionary<string, double> SummarizeTrainingDueSlotsForPreference(List<TrainingDueSlot> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedLoad();
        }
        return totals;
    }

    public static List<RegionalAttestationPreference> DeriveRegionalAttestationPreferencesFromSlot(List<TrainingDueSlot> items)
    {
        var outValue = new List<RegionalAttestationPreference>();
        foreach (var item in items)
        {
            var derived = RegionalAttestationPreference.NewRegionalAttestationPreference(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string BalanceRegionalAttestationPreference(RegionalAttestationPreference item)
    {
        return PrimaryTrainingDuePlanPolicy.PrimaryTrainingDuePlanBucket(item.WeightedLoad());
    }
}
