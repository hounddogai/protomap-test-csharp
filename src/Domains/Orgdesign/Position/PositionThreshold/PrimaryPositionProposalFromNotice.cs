namespace Acme.Workforce.Orgdesign.Position.PositionThreshold;

using Acme.Workforce.Orgdesign.Span.Window;

/// <summary>Functions of primary position proposal from notice.</summary>
public static class PrimaryPositionProposalFromNotice
{
    public static Dictionary<string, double> SummarizeSpanNoticesForProposal(List<SpanNotice> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedLoad();
        }
        return totals;
    }

    public static List<PrimaryPositionProposal> DerivePrimaryPositionProposalsFromNotice(List<SpanNotice> items)
    {
        var outValue = new List<PrimaryPositionProposal>();
        foreach (var item in items)
        {
            var derived = PrimaryPositionProposal.NewPrimaryPositionProposal(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ReconcilePrimaryPositionProposal(PrimaryPositionProposal item)
    {
        return StrictSpanProposalPolicy.StrictSpanProposalWeight(9, true);
    }
}
