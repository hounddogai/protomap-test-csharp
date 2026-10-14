namespace Acme.Workforce.Workplace.Tests.Desk.DeskRecord;

using Acme.Workforce.Workplace.Desk.DeskRecord;

/// <summary>Checks of desk record code, run by the project's runner.</summary>
public static class PendingDeskBaselineViewChecks
{
    public static void PendingDeskBaselineViewShowsTheStatusInCapitals()
    {
        var item = PendingDeskBaseline.NewPendingDeskBaseline("code-666", "name-638");
        var view = PendingDeskBaselineView.ToPendingDeskBaselineView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingDeskBaselineView.ToPendingDeskBaselineViews(new List<PendingDeskBaseline> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingDeskBaselineViewShowsTheStatusInCapitals();
    }
}
