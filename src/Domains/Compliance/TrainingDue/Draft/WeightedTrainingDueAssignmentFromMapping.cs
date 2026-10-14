namespace Acme.Workforce.Compliance.TrainingDue.Draft;

using Acme.Workforce.Compliance.Audit.Claim;

/// <summary>Functions of weighted training due assignment from mapping.</summary>
public static class WeightedTrainingDueAssignmentFromMapping
{
    public static Dictionary<string, double> SummarizeManualAuditMappingsForAssignment(List<ManualAuditMapping> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveTotal();
        }
        return totals;
    }

    public static List<WeightedTrainingDueAssignment> DeriveWeightedTrainingDueAssignmentsFromMapping(List<ManualAuditMapping> items)
    {
        var outValue = new List<WeightedTrainingDueAssignment>();
        foreach (var item in items)
        {
            var derived = WeightedTrainingDueAssignment.NewWeightedTrainingDueAssignment(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AllocateWeightedTrainingDueAssignment(WeightedTrainingDueAssignment item)
    {
        return LenientAuditPlanPolicy.ClassifyLenientAuditPlan(item.MeasuredValue());
    }
}
