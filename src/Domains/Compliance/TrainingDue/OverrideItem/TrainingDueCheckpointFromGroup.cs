namespace Acme.Workforce.Compliance.TrainingDue.OverrideItem;

using Acme.Workforce.Compliance.Exception.Group;

/// <summary>Functions of training due checkpoint from group.</summary>
public static class TrainingDueCheckpointFromGroup
{
    public static Dictionary<string, double> SummarizeExceptionGroupsForCheckpoint(List<ExceptionGroup> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedTotal();
        }
        return totals;
    }

    public static List<TrainingDueCheckpoint> DeriveTrainingDueCheckpointsFromGroup(List<ExceptionGroup> items)
    {
        var outValue = new List<TrainingDueCheckpoint>();
        foreach (var item in items)
        {
            var derived = TrainingDueCheckpoint.NewTrainingDueCheckpoint(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolveTrainingDueCheckpoint(TrainingDueCheckpoint item)
    {
        return ExceptionPlanPolicy.ClassifyExceptionPlan(item.ProjectedExposure());
    }
}
