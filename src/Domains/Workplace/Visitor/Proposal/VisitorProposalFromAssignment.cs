namespace Acme.Workforce.Workplace.Visitor.Proposal;

using Acme.Workforce.Workplace.AccessBadge.Bundle;

/// <summary>Functions of visitor proposal from assignment.</summary>
public static class VisitorProposalFromAssignment
{
    public static Dictionary<string, double> SummarizeAccessBadgeAssignmentsForProposal(List<AccessBadgeAssignment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetExposure();
        }
        return totals;
    }

    public static List<VisitorProposal> DeriveVisitorProposalsFromAssignment(List<AccessBadgeAssignment> items)
    {
        var outValue = new List<VisitorProposal>();
        foreach (var item in items)
        {
            var derived = VisitorProposal.NewVisitorProposal(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyVisitorProposalExtra(VisitorProposal item)
    {
        return AccessBadgeAssignmentPolicy.NormalizeAccessBadgeAssignmentCode(item.Code);
    }
}
