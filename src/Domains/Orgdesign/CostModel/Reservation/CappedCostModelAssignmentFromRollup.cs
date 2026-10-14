namespace Acme.Workforce.Orgdesign.CostModel.Reservation;

using Acme.Workforce.Orgdesign.Position.Policy;

/// <summary>Functions of capped cost model assignment from rollup.</summary>
public static class CappedCostModelAssignmentFromRollup
{
    public static Dictionary<string, double> SummarizePositionRollupsForAssignment(List<PositionRollup> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedValue();
        }
        return totals;
    }

    public static List<CappedCostModelAssignment> DeriveCappedCostModelAssignmentsFromRollup(List<PositionRollup> items)
    {
        var outValue = new List<CappedCostModelAssignment>();
        foreach (var item in items)
        {
            var derived = CappedCostModelAssignment.NewCappedCostModelAssignment(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveCappedCostModelAssignment(CappedCostModelAssignment item)
    {
        return PositionPolicyPolicy.PositionPolicyBucket(item.ProjectedValue());
    }
}
