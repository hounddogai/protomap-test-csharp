namespace Acme.Workforce.Contractors.StatementOfWork.Assignment;

using Acme.Workforce.Contractors.Engagement.Assignment;

/// <summary>Functions of statement of work proposal from assignment.</summary>
public static class StatementOfWorkProposalFromAssignment
{
    public static Dictionary<string, double> SummarizeEngagementAssignmentsForProposal(List<EngagementAssignment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedTotal();
        }
        return totals;
    }

    public static List<StatementOfWorkProposal> DeriveStatementOfWorkProposalsFromAssignment(List<EngagementAssignment> items)
    {
        var outValue = new List<StatementOfWorkProposal>();
        foreach (var item in items)
        {
            var derived = StatementOfWorkProposal.NewStatementOfWorkProposal(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeStatementOfWorkProposal(StatementOfWorkProposal item)
    {
        return EngagementPreferencePolicy.ClassifyEngagementPreference(item.ProjectedTotal());
    }
}
