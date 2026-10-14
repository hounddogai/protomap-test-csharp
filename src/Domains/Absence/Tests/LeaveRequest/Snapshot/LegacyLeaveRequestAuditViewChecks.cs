namespace Acme.Workforce.Absence.Tests.LeaveRequest.Snapshot;

using Acme.Workforce.Absence.LeaveRequest.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class LegacyLeaveRequestAuditViewChecks
{
    public static void LegacyLeaveRequestAuditViewShowsTheStatusInCapitals()
    {
        var item = LegacyLeaveRequestAudit.NewLegacyLeaveRequestAudit("code-997", "label-644");
        var view = LegacyLeaveRequestAuditView.ToLegacyLeaveRequestAuditView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LegacyLeaveRequestAuditView.ToLegacyLeaveRequestAuditViews(new List<LegacyLeaveRequestAudit> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LegacyLeaveRequestAuditViewShowsTheStatusInCapitals();
    }
}
