namespace Acme.Workforce.Compliance.TrainingDue.Schedule;

using Acme.Workforce.Compliance.TrainingDue.OverrideItem;

/// <summary>Functions of training due schedule from audit.</summary>
public static class TrainingDueScheduleFromAudit
{
    public static Dictionary<string, double> SummarizeTieredTrainingDueAuditsForSchedule(List<TieredTrainingDueAudit> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetValue();
        }
        return totals;
    }

    public static List<TrainingDueSchedule> DeriveTrainingDueSchedulesFromAudit(List<TieredTrainingDueAudit> items)
    {
        var outValue = new List<TrainingDueSchedule>();
        foreach (var item in items)
        {
            var derived = TrainingDueSchedule.NewTrainingDueSchedule(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AllocateTrainingDueSchedule(TrainingDueSchedule item)
    {
        return TieredTrainingDueAuditPolicy.NormalizeTieredTrainingDueAuditCode(item.Code);
    }
}
