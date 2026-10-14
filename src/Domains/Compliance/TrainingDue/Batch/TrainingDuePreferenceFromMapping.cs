namespace Acme.Workforce.Compliance.TrainingDue.Batch;

using Acme.Workforce.Compliance.Audit.AuditMapping;

/// <summary>Functions of training due preference from mapping.</summary>
public static class TrainingDuePreferenceFromMapping
{
    public static Dictionary<string, double> SummarizeDefaultAuditMappingsForPreference(List<DefaultAuditMapping> items)
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

    public static List<TrainingDuePreference> DeriveTrainingDuePreferencesFromMapping(List<DefaultAuditMapping> items)
    {
        var outValue = new List<TrainingDuePreference>();
        foreach (var item in items)
        {
            var derived = TrainingDuePreference.NewTrainingDuePreference(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveTrainingDuePreference(TrainingDuePreference item)
    {
        return AuditLedgerPolicy.AuditLedgerBucket(item.MeasuredValue());
    }
}
