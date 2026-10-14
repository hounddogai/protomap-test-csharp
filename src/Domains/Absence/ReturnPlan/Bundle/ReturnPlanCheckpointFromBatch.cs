namespace Acme.Workforce.Absence.ReturnPlan.Bundle;

using Acme.Workforce.Absence.LeaveRequest.Budget;

/// <summary>Functions of return plan checkpoint from batch.</summary>
public static class ReturnPlanCheckpointFromBatch
{
    public static Dictionary<string, double> SummarizeLeaveRequestBatchesForCheckpoint(List<LeaveRequestBatch> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetTotal();
        }
        return totals;
    }

    public static List<ReturnPlanCheckpoint> DeriveReturnPlanCheckpointsFromBatch(List<LeaveRequestBatch> items)
    {
        var outValue = new List<ReturnPlanCheckpoint>();
        foreach (var item in items)
        {
            var derived = ReturnPlanCheckpoint.NewReturnPlanCheckpoint(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectReturnPlanCheckpoint(ReturnPlanCheckpoint item)
    {
        return LeaveRequestRulePolicy.ClassifyLeaveRequestRule(item.EffectiveTotal());
    }
}
