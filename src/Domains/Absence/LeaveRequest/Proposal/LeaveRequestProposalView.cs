namespace Acme.Workforce.Absence.LeaveRequest.Proposal;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a leave request proposal.</summary>
public sealed record LeaveRequestProposalView(string CodeText, string Label, string AttemptsText, string Department, string Phone, string ExpiresAt, string ExternalRef, string StatusLabel)
{
    public static LeaveRequestProposalView ToLeaveRequestProposalView(LeaveRequestProposal item)
    {
        return new LeaveRequestProposalView(
            item.Code,
            Text.Truncate(item.Label, 64),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Department,
            item.Phone,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.ExternalRef,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LeaveRequestProposalView> ToLeaveRequestProposalViews(List<LeaveRequestProposal> items)
    {
        var views = new List<LeaveRequestProposalView>();
        foreach (var item in items)
        {
            views.Add(ToLeaveRequestProposalView(item));
        }
        return views;
    }

    public static double SumLeaveRequestProposalView(LeaveRequestProposalView view)
    {
        var parsedAttempts = int.Parse(view.AttemptsText, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + (double)parsedExpiresAt;
    }
}
