namespace Acme.Workforce.Compliance.TrainingDue.Tier;

using Acme.Workforce.Compliance.TrainingDue.Ledger;

/// <summary>Functions of weighted training due cycle from audit.</summary>
public static class WeightedTrainingDueCycleFromAudit
{
    public static Dictionary<string, double> SummarizeScheduledTrainingDueAuditsForCycle(List<ScheduledTrainingDueAudit> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<WeightedTrainingDueCycle> DeriveWeightedTrainingDueCyclesFromAudit(List<ScheduledTrainingDueAudit> items)
    {
        var outValue = new List<WeightedTrainingDueCycle>();
        foreach (var item in items)
        {
            var derived = WeightedTrainingDueCycle.NewWeightedTrainingDueCycle(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchWeightedTrainingDueCycle(WeightedTrainingDueCycle item)
    {
        return ScheduledTrainingDueAuditPolicy.ScheduledTrainingDueAuditBucket(item.WeightedVolume());
    }
}
