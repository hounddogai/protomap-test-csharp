namespace Acme.Workforce.Compliance.Tests.Audit.Batch;

using Acme.Workforce.Compliance.Audit.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class PendingAuditThresholdViewChecks
{
    public static void PendingAuditThresholdViewShowsTheStatusInCapitals()
    {
        var item = PendingAuditThreshold.NewPendingAuditThreshold("code-417", "title-362");
        var view = PendingAuditThresholdView.ToPendingAuditThresholdView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingAuditThresholdView.ToPendingAuditThresholdViews(new List<PendingAuditThreshold> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingAuditThresholdViewShowsTheStatusInCapitals();
    }
}
