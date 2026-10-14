namespace Acme.Workforce.Compensation.Adjustment.Mapping;

using Acme.Workforce.Compensation.Adjustment.Score;

/// <summary>Functions of primary adjustment assignment from plan.</summary>
public static class PrimaryAdjustmentAssignmentFromPlan
{
    public static Dictionary<string, double> SummarizeAdjustmentPlansForAssignment(List<AdjustmentPlan> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveValue();
        }
        return totals;
    }

    public static List<PrimaryAdjustmentAssignment> DerivePrimaryAdjustmentAssignmentsFromPlan(List<AdjustmentPlan> items)
    {
        var outValue = new List<PrimaryAdjustmentAssignment>();
        foreach (var item in items)
        {
            var derived = PrimaryAdjustmentAssignment.NewPrimaryAdjustmentAssignment(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankPrimaryAdjustmentAssignment(PrimaryAdjustmentAssignment item)
    {
        return AdjustmentTemplatePolicy.AdjustmentTemplateBucket(item.EffectiveValue());
    }
}
