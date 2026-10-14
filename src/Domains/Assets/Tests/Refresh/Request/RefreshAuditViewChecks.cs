namespace Acme.Workforce.Assets.Tests.Refresh.Request;

using Acme.Workforce.Assets.Refresh.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class RefreshAuditViewChecks
{
    public static void RefreshAuditViewShowsTheStatusInCapitals()
    {
        var item = RefreshAudit.NewRefreshAudit("code-587", "title-538");
        var view = RefreshAuditView.ToRefreshAuditView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RefreshAuditView.ToRefreshAuditViews(new List<RefreshAudit> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RefreshAuditViewShowsTheStatusInCapitals();
    }
}
