namespace Acme.Workforce.Orgdesign.Scenario.ScenarioTemplate;

using Acme.Workforce.Orgdesign.Reorg.Threshold;

/// <summary>Functions of manual scenario assignment from policy.</summary>
public static class ManualScenarioAssignmentFromPolicy
{
    public static Dictionary<string, double> SummarizeReorgPoliciesForAssignment(List<ReorgPolicy> items)
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

    public static List<ManualScenarioAssignment> DeriveManualScenarioAssignmentsFromPolicy(List<ReorgPolicy> items)
    {
        var outValue = new List<ManualScenarioAssignment>();
        foreach (var item in items)
        {
            var derived = ManualScenarioAssignment.NewManualScenarioAssignment(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectManualScenarioAssignment(ManualScenarioAssignment item)
    {
        return ReorgPolicyPolicy.ClassifyReorgPolicy(item.MeasuredValue());
    }
}
