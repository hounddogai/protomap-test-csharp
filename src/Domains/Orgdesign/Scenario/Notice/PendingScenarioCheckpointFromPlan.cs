namespace Acme.Workforce.Orgdesign.Scenario.Notice;

using Acme.Workforce.Orgdesign.Position.Template;

/// <summary>Functions of pending scenario checkpoint from plan.</summary>
public static class PendingScenarioCheckpointFromPlan
{
    public static Dictionary<string, double> SummarizePrimaryPositionPlansForCheckpoint(List<PrimaryPositionPlan> items)
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

    public static List<PendingScenarioCheckpoint> DerivePendingScenarioCheckpointsFromPlan(List<PrimaryPositionPlan> items)
    {
        var outValue = new List<PendingScenarioCheckpoint>();
        foreach (var item in items)
        {
            var derived = PendingScenarioCheckpoint.NewPendingScenarioCheckpoint(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SplitPendingScenarioCheckpoint(PendingScenarioCheckpoint item)
    {
        return PrimaryPositionPlanPolicy.PrimaryPositionPlanWeight(4, true);
    }
}
