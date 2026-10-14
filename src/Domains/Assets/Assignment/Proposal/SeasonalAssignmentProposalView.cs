namespace Acme.Workforce.Assets.Assignment.Proposal;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a seasonal assignment proposal.</summary>
public sealed record SeasonalAssignmentProposalView(string CodeText, string Name, string DurationMinutesText, string EndsAtText, string ApprovedText, string Hours, string Minimum, string CostCenter, string Country, string StatusLabel)
{
    public static SeasonalAssignmentProposalView ToSeasonalAssignmentProposalView(SeasonalAssignmentProposal item)
    {
        return new SeasonalAssignmentProposalView(
            Text.Truncate(item.Code, 120),
            item.Name,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.CostCenter,
            Text.Truncate(item.Country, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalAssignmentProposalView> ToSeasonalAssignmentProposalViews(List<SeasonalAssignmentProposal> items)
    {
        var views = new List<SeasonalAssignmentProposalView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalAssignmentProposalView(item));
        }
        return views;
    }

    public static double SumSeasonalAssignmentProposalView(SeasonalAssignmentProposalView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutesText, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAtText, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + (double)parsedEndsAt + parsedHours;
    }
}
