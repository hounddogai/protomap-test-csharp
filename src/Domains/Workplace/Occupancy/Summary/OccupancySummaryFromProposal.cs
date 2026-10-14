namespace Acme.Workforce.Workplace.Occupancy.Summary;

using Acme.Workforce.Workplace.Desk.Proposal;

/// <summary>Functions of occupancy summary from proposal.</summary>
public static class OccupancySummaryFromProposal
{
    public static Dictionary<string, double> SummarizeDeskProposalsForSummary(List<DeskProposal> items)
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

    public static List<OccupancySummary> DeriveOccupancySummariesFromProposal(List<DeskProposal> items)
    {
        var outValue = new List<OccupancySummary>();
        foreach (var item in items)
        {
            var derived = OccupancySummary.NewOccupancySummary(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolveOccupancySummary(OccupancySummary item)
    {
        return DeskAuditPolicy.ClassifyDeskAudit(item.EffectiveExposure());
    }
}
