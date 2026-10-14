namespace Acme.Workforce.Compliance.Tests.Audit.Assignment;

using Acme.Workforce.Compliance.Audit.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class GlobalAuditCheckpointViewChecks
{
    public static void GlobalAuditCheckpointViewShowsTheStatusInCapitals()
    {
        var item = GlobalAuditCheckpoint.NewGlobalAuditCheckpoint("code-745", "name-544");
        var view = GlobalAuditCheckpointView.ToGlobalAuditCheckpointView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(GlobalAuditCheckpointView.ToGlobalAuditCheckpointViews(new List<GlobalAuditCheckpoint> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        GlobalAuditCheckpointViewShowsTheStatusInCapitals();
    }
}
