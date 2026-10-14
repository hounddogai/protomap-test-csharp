namespace Acme.Workforce.Contractors.Rate.Proposal;

using Acme.Workforce.Contractors.StatementOfWork.Agreement;

/// <summary>Functions of strict rate proposal from draft.</summary>
public static class StrictRateProposalFromDraft
{
    public static Dictionary<string, double> SummarizeStatementOfWorkDraftsForProposal(List<StatementOfWorkDraft> items)
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

    public static List<StrictRateProposal> DeriveStrictRateProposalsFromDraft(List<StatementOfWorkDraft> items)
    {
        var outValue = new List<StrictRateProposal>();
        foreach (var item in items)
        {
            var derived = StrictRateProposal.NewStrictRateProposal(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcileStrictRateProposal(StrictRateProposal item)
    {
        return StatementOfWorkDraftPolicy.ClassifyStatementOfWorkDraft(item.MeasuredValue());
    }
}
