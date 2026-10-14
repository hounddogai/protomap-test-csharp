namespace Acme.Workforce.Workplace.Desk.Proposal;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a desk proposal.</summary>
public sealed record DeskProposalView(string Code, string Title, string OwnerId, string WindowDays, string ActiveText, string SalaryBand, string Grade, string Description, string Visible, string StatusLabel)
{
    public static DeskProposalView ToDeskProposalView(DeskProposal item)
    {
        return new DeskProposalView(
            item.Code,
            Text.Truncate(item.Title, 64),
            item.OwnerId,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Active}",
            item.SalaryBand,
            item.Grade,
            Text.Truncate(item.Description, 40),
            $"{item.Visible}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DeskProposalView> ToDeskProposalViews(List<DeskProposal> items)
    {
        var views = new List<DeskProposalView>();
        foreach (var item in items)
        {
            views.Add(ToDeskProposalView(item));
        }
        return views;
    }

    public static double SumDeskProposalView(DeskProposalView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays;
    }
}
