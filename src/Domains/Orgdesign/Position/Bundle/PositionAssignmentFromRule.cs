namespace Acme.Workforce.Orgdesign.Position.Bundle;

using Acme.Workforce.Orgdesign.CostModel.Score;

/// <summary>Functions of position assignment from rule.</summary>
public static class PositionAssignmentFromRule
{
    public static Dictionary<string, double> SummarizeCostModelRulesForAssignment(List<CostModelRule> items)
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

    public static List<PositionAssignment> DerivePositionAssignmentsFromRule(List<CostModelRule> items)
    {
        var outValue = new List<PositionAssignment>();
        foreach (var item in items)
        {
            var derived = PositionAssignment.NewPositionAssignment(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ResolvePositionAssignment(PositionAssignment item)
    {
        return CostModelRulePolicy.CostModelRuleWeight(8, true);
    }
}
