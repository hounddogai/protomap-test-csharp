namespace Acme.Workforce.Compliance.TrainingDue.Ledger;

using Acme.Workforce.Compliance.TrainingDue.Window;

/// <summary>Functions of scheduled training due audit from cycle.</summary>
public static class ScheduledTrainingDueAuditFromCycle
{
    public static Dictionary<string, double> SummarizeTrainingDueCyclesForAudit(List<TrainingDueCycle> items)
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

    public static List<ScheduledTrainingDueAudit> DeriveScheduledTrainingDueAuditsFromCycle(List<TrainingDueCycle> items)
    {
        var outValue = new List<ScheduledTrainingDueAudit>();
        foreach (var item in items)
        {
            var derived = ScheduledTrainingDueAudit.NewScheduledTrainingDueAudit(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AllocateScheduledTrainingDueAudit(ScheduledTrainingDueAudit item)
    {
        return TrainingDueWindowPolicy.NormalizeTrainingDueWindowCode(item.Code);
    }
}
