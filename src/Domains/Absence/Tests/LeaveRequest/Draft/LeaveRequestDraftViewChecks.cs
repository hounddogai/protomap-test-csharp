namespace Acme.Workforce.Absence.Tests.LeaveRequest.Draft;

using Acme.Workforce.Absence.LeaveRequest.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class LeaveRequestDraftViewChecks
{
    public static void LeaveRequestDraftViewShowsTheStatusInCapitals()
    {
        var item = LeaveRequestDraft.NewLeaveRequestDraft("code-558", "title-175");
        var view = LeaveRequestDraftView.ToLeaveRequestDraftView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LeaveRequestDraftView.ToLeaveRequestDraftViews(new List<LeaveRequestDraft> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LeaveRequestDraftViewShowsTheStatusInCapitals();
    }
}
