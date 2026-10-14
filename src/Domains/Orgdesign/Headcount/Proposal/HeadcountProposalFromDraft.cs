namespace Acme.Workforce.Orgdesign.Headcount.Proposal;

using Acme.Workforce.Orgdesign.Headcount.Draft;

/// <summary>Functions of headcount proposal from draft.</summary>
public static class HeadcountProposalFromDraft
{
    public static Dictionary<string, double> SummarizePrimaryHeadcountDraftsForProposal(List<PrimaryHeadcountDraft> items)
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

    public static List<HeadcountProposal> DeriveHeadcountProposalsFromDraft(List<PrimaryHeadcountDraft> items)
    {
        var outValue = new List<HeadcountProposal>();
        foreach (var item in items)
        {
            var derived = HeadcountProposal.NewHeadcountProposal(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SettleHeadcountProposal(HeadcountProposal item)
    {
        return PrimaryHeadcountDraftPolicy.PrimaryHeadcountDraftBucket(item.NetVolume());
    }
}
