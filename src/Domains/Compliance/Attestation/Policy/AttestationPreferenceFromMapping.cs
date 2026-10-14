namespace Acme.Workforce.Compliance.Attestation.Policy;

using Acme.Workforce.Compliance.TrainingDue.DueSchedule;

/// <summary>Functions of attestation preference from mapping.</summary>
public static class AttestationPreferenceFromMapping
{
    public static Dictionary<string, double> SummarizeBatchedTrainingDueMappingsForPreference(List<BatchedTrainingDueMapping> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedTotal();
        }
        return totals;
    }

    public static List<AttestationPreference> DeriveAttestationPreferencesFromMapping(List<BatchedTrainingDueMapping> items)
    {
        var outValue = new List<AttestationPreference>();
        foreach (var item in items)
        {
            var derived = AttestationPreference.NewAttestationPreference(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizeAttestationPreference(AttestationPreference item)
    {
        return LenientTrainingDueSchedulePolicy.NormalizeLenientTrainingDueScheduleCode(item.Code);
    }
}
