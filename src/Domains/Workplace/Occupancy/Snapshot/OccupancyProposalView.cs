namespace Acme.Workforce.Workplace.Occupancy.Snapshot;

using System.Globalization;

/// <summary>How screens and exports show a occupancy proposal.</summary>
public sealed record OccupancyProposalView(string CodeText, string Label, string LimitText, string Archived, string SummaryText, string Fte, string JobCodeText, string Location, string StatusLabel)
{
    public static OccupancyProposalView ToOccupancyProposalView(OccupancyProposal item)
    {
        return new OccupancyProposalView(
            item.Code,
            item.Label,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            $"{item.Archived}",
            item.Summary,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.JobCode,
            item.Location,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<OccupancyProposalView> ToOccupancyProposalViews(List<OccupancyProposal> items)
    {
        var views = new List<OccupancyProposalView>();
        foreach (var item in items)
        {
            views.Add(ToOccupancyProposalView(item));
        }
        return views;
    }

    public static double SumOccupancyProposalView(OccupancyProposalView view)
    {
        var parsedLimit = int.Parse(view.LimitText, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        return (double)parsedLimit + parsedFte;
    }
}
