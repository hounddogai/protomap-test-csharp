namespace Acme.Workforce.Workplace.Tests.Desk.Proposal;

using Acme.Workforce.Workplace.Desk.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class DeskAuditViewChecks
{
    public static void DeskAuditViewShowsTheStatusInCapitals()
    {
        var item = DeskAudit.NewDeskAudit("code-610", "label-231");
        var view = DeskAuditView.ToDeskAuditView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DeskAuditView.ToDeskAuditViews(new List<DeskAudit> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DeskAuditViewShowsTheStatusInCapitals();
    }
}
