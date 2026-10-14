namespace Acme.Workforce.Compliance.Tests.Audit.Checkpoint;

using Acme.Workforce.Compliance.Audit.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class AuditCheckpointViewChecks
{
    public static void AuditCheckpointViewShowsTheStatusInCapitals()
    {
        var item = AuditCheckpoint.NewAuditCheckpoint("code-551", "name-827");
        var view = AuditCheckpointView.ToAuditCheckpointView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AuditCheckpointView.ToAuditCheckpointViews(new List<AuditCheckpoint> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AuditCheckpointViewShowsTheStatusInCapitals();
    }
}
