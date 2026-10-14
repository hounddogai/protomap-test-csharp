namespace Acme.Workforce.Contractors.Engagement.Policy;

using Acme.Workforce.Contractors.Timesheet.Audit;

/// <summary>Functions of lenient engagement policy from audit.</summary>
public static class LenientEngagementPolicyFromAudit
{
    public static Dictionary<string, double> SummarizeRegionalTimesheetAuditsForPolicy(List<RegionalTimesheetAudit> items)
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

    public static List<LenientEngagementPolicy> DeriveLenientEngagementPoliciesFromAudit(List<RegionalTimesheetAudit> items)
    {
        var outValue = new List<LenientEngagementPolicy>();
        foreach (var item in items)
        {
            var derived = LenientEngagementPolicy.NewLenientEngagementPolicy(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ClassifyLenientEngagementPolicyExtra(LenientEngagementPolicy item)
    {
        return PendingTimesheetSummaryPolicy.PendingTimesheetSummaryWeight(1, true);
    }
}
