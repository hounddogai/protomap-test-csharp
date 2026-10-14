namespace Acme.Workforce.Compliance.Tests.Audit.Review;

using Acme.Workforce.Compliance.Audit.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class DefaultAuditCheckpointViewChecks
{
    public static void DefaultAuditCheckpointViewShowsTheStatusInCapitals()
    {
        var item = DefaultAuditCheckpoint.NewDefaultAuditCheckpoint("code-816", "name-623");
        var view = DefaultAuditCheckpointView.ToDefaultAuditCheckpointView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultAuditCheckpointView.ToDefaultAuditCheckpointViews(new List<DefaultAuditCheckpoint> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultAuditCheckpointViewShowsTheStatusInCapitals();
    }
}
