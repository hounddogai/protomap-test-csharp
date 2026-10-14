namespace Acme.Workforce.Absence.Tests.LeaveRequest.Batch;

using Acme.Workforce.Absence.LeaveRequest.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class PendingLeaveRequestBatchViewChecks
{
    public static void PendingLeaveRequestBatchViewShowsTheStatusInCapitals()
    {
        var item = PendingLeaveRequestBatch.NewPendingLeaveRequestBatch("code-711", "title-999");
        var view = PendingLeaveRequestBatchView.ToPendingLeaveRequestBatchView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingLeaveRequestBatchView.ToPendingLeaveRequestBatchViews(new List<PendingLeaveRequestBatch> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingLeaveRequestBatchViewShowsTheStatusInCapitals();
    }
}
