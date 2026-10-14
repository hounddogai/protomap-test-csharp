namespace Acme.Workforce.Contractors.Engagement.Template;

using Acme.Workforce.Contractors.StatementOfWork.WorkAssignment;

/// <summary>Functions of manual engagement rule from assignment.</summary>
public static class ManualEngagementRuleFromAssignment
{
    public static Dictionary<string, double> SummarizeCappedStatementOfWorkAssignmentsForRule(List<CappedStatementOfWorkAssignment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedLoad();
        }
        return totals;
    }

    public static List<ManualEngagementRule> DeriveManualEngagementRulesFromAssignment(List<CappedStatementOfWorkAssignment> items)
    {
        var outValue = new List<ManualEngagementRule>();
        foreach (var item in items)
        {
            var derived = ManualEngagementRule.NewManualEngagementRule(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizeManualEngagementRule(ManualEngagementRule item)
    {
        return RollingStatementOfWorkAllocationPolicy.ClassifyRollingStatementOfWorkAllocation(item.ProjectedVolume());
    }
}
